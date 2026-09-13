using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Responses;
using Samples.SampleUtilities;
using System.ClientModel;
using System.Text;

namespace Samples.Section05;

public static class WebSearch
{
    public static async Task RunSample()
    {
        //Create Raw Connection
        (string endpoint, string apiKey) = SecretManager.GetAzureOpenAIApiKeyBasedCredentials();
        OpenAIClient client = new OpenAIClient(new ApiKeyCredential(apiKey), new OpenAIClientOptions
        {
            Endpoint = new Uri(endpoint + "/openai/v1")
        });

        //Create Agent
#pragma warning disable OPENAI001
        ChatClientAgent agent = client
            .GetResponsesClient()
#pragma warning restore OPENAI001
            .AsAIAgent(
                model: "gpt-4.1-nano",
                tools:
                [
                    AIFunctionFactory.Create(GetDateTimeUtc),
                    AIFunctionFactory.Create(GetTimeZoneInfo),
                    new HostedWebSearchTool()
                ],
                instructions: "You are a Space news Agent (Always in include today's date at the top of your answers)");

        AgentSession session = await agent.CreateSessionAsync();

        Console.OutputEncoding = Encoding.UTF8;
        while (true)
        {
            Console.Write("> ");
            string input = Console.ReadLine() ?? "";
            AgentResponse response = await agent.RunAsync(input, session);
            {
                Console.WriteLine(response);
            }

            Output.Separator();
        }
    }


    public static DateTime GetDateTimeUtc()
    {
        return DateTime.UtcNow;
    }

    public static TimeZoneInfo GetTimeZoneInfo()
    {
        return TimeZoneInfo.Local;
    }

    //Note:
    //Web search cost $10.00 / 1K calls + search content tokens billed at model rates
}