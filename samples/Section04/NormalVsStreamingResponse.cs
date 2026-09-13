using Microsoft.Agents.AI;
using OpenAI;
using OpenAI.Chat;
using Samples.SampleUtilities;
using System.ClientModel;

namespace Samples.Section04;

public static class NormalVsStreamingResponse
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
        ChatClientAgent agent = client.GetChatClient("gpt-4.1-nano").AsAIAgent();

        Output.Title("Normal Call");
        AgentResponse response = await agent.RunAsync("What is the Capital of France?");
        Console.WriteLine(response);

        Output.Separator();

        Output.Title("Streaming Call");
        await foreach (AgentResponseUpdate update in agent.RunStreamingAsync("How to make soup?"))
        {
            Console.Write(update);
        }

        Output.Separator();

        Output.Title("Streaming Call (gathering all updates to a response at the end)");
        List<AgentResponseUpdate> updates = [];
        await foreach (AgentResponseUpdate update in agent.RunStreamingAsync("How to make soup?"))
        {
            updates.Add(update);
            Console.Write(update);
        }

        AgentResponse collectedResponse = updates.ToAgentResponse();
        //Use to the usage, and other return data...
        Console.WriteLine(collectedResponse.Usage!.OutputTokenCount);
    }
}