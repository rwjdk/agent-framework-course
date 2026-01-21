using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using Samples.SampleUtilities;
using System.ClientModel;
using System.Text;

namespace Samples.Section05;

public static class ToolCallingMiddleware
{
    public static async Task RunSample()
    {
        //Create Raw Connection
        (string endpoint, string apiKey) = SecretManager.GetAzureOpenAIApiKeyBasedCredentials();
        AzureOpenAIClient client = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));

        PersonTools personTools = new PersonTools();

        //Create Agent
        AIAgent agent = client
            .GetChatClient("gpt-4.1-nano")
            .AsAIAgent(
                instructions: "You know people and can change colors of the Console ('change_console_color')",
                tools:
                [
                    AIFunctionFactory.Create(personTools.GetPersons, "get_persons", "Get all the persons you know"),
                    AIFunctionFactory.Create(personTools.GetPerson, "get_person", "Get a specific person by name"),
                    AIFunctionFactory.Create(ChangeConsoleColor, "change_console_color", "Change the color of the console")
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
        toolDetails.Append($"- Tool Call: '{context.Function.Name}'");
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

    public record PersonInfo(string Name, string FavoriteColor);

    //Information Tool
    public class PersonTools
    {
        public PersonInfo[] GetPersons()
        {
            return GetData();
        }

        public PersonInfo? GetPerson(string name)
        {
            PersonInfo[] data = GetData();
            return data.FirstOrDefault(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
        }

        private static PersonInfo[] GetData()
        {
            return
            [
                new PersonInfo("Rasmus", "Blue"),
                new PersonInfo("John", "Red"),
                new PersonInfo("Ben", "Green"),
                new PersonInfo("Jenny", "Red"),
                new PersonInfo("Mona", "Yellow"),
            ];
        }
    }

    //Action Tool
    public static void ChangeConsoleColor(ConsoleColor color)
    {
        Console.ForegroundColor = color;
    }
}