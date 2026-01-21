using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using OpenAI.Chat;
using Samples.SampleUtilities;
using System.ClientModel;
using System.Text;
using Microsoft.Extensions.AI;
using OpenAI.Responses;

namespace Samples.Section05;

public static class WebSearch
{
    public static async Task RunSample()
    {
        //Create Raw Connection
        (string endpoint, string apiKey) = SecretManager.GetAzureOpenAIApiKeyBasedCredentials();
        AzureOpenAIClient client = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));

        //Create Agent
#pragma warning disable OPENAI001
        ChatClientAgent agent = client
            .GetResponsesClient("gpt-4.1-nano")
#pragma warning restore OPENAI001
            .AsAIAgent(
                tools:
                [
                    AIFunctionFactory.Create(GetDateTimeUtc),
                    AIFunctionFactory.Create(GetTimeZoneInfo),
                    new HostedWebSearchTool()
                ],
                instructions: "You are a Space news Agent (Always in include today's date at the top of your answers)");

        AgentThread thread = await agent.GetNewThreadAsync();;

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