using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using Samples.SampleUtilities;
using System.ClientModel;
using System.Text;

namespace Samples.Section05;

public static class OtherAgentsAsTools
{
    public static async Task RunSample()
    {
        //Create Raw Connection
        (string endpoint, string apiKey) = SecretManager.GetAzureOpenAIApiKeyBasedCredentials();
        AzureOpenAIClient client = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));

        ChatClientAgent astronomyAgent = client
            .GetChatClient("gpt-5.2")
            .AsAIAgent(
                name: "AstronomyAgent",
                instructions: "You an expert in Astronomy");

        AIAgent agent = client
            .GetChatClient("gpt-4.1-nano")
            .AsAIAgent(
                name: "MainAgent",
                instructions: "Refer all astronomy questions to the 'AstronomyAgent'",
                tools:
                [
                    astronomyAgent.AsAIFunction(),
                ])
            .AsBuilder()
            .Use(Middleware)
            .Build();

        AgentThread thread = await agent.GetNewThreadAsync();

        Console.OutputEncoding = Encoding.UTF8;
        while (true)
        {
            Console.Write("> ");
            string input = Console.ReadLine() ?? "";
            AgentResponse response = await agent.RunAsync(input, thread);
            {
                Console.WriteLine(response);
            }

            Output.Separator();
        }
    }

    private static async ValueTask<object?> Middleware(AIAgent agent, FunctionInvocationContext context,
        Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next, CancellationToken cancellationToken)
    {
        StringBuilder toolDetails = new();
        toolDetails.Append($"- Tool Call: [Agent={agent.Name}] [Tool={context.Function.Name}]");
        if (context.Arguments.Count > 0)
        {
            toolDetails.Append($" (Args: {string.Join(",", context.Arguments.Select(x => $"[{x.Key} = {x.Value}]"))}");
        }

        Output.Yellow(toolDetails.ToString());

        if (context.Function.Name == "get_person")
        {
            if (context.Arguments.Any(x => x.Value!.ToString()!.Equals("John", StringComparison.CurrentCultureIgnoreCase)))
            {
                throw new Exception("John's data is secret!");
            }

            if (context.Arguments.Any(x => x.Value!.ToString()!.Equals("Ben", StringComparison.CurrentCultureIgnoreCase)))
            {
                return "His favorite color is Orange with a hint of black stripes";
            }
        }

        return await next.Invoke(context, cancellationToken);
    }

    public static class StringTools
    {
        public static string Uppercase(string input)
        {
            return input.ToUpper();
        }

        public static string Lowercase(string input)
        {
            return input.ToLower();
        }

        public static string Reverse(string input)
        {
            return new string(input.ToCharArray().Reverse().ToArray());
        }
    }

    public static class NumberTools
    {
        public static int AnswerToEverythingNumber()
        {
            return 42;
        }

        public static int RandomNumber(int min = 0, int max = 100)
        {
            return Random.Shared.Next(min, max + 1);
        }
    }
}