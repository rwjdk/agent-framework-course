using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Samples.SampleUtilities;
using System.ClientModel;
using OpenAI.Chat;

namespace Samples.Section04;

public static class NormalVsStreamingResponse
{
    public static async Task RunSample()
    {
        //Create Raw Connection
        (string endpoint, string apiKey) = SecretManager.GetAzureOpenAIApiKeyBasedCredentials();
        AzureOpenAIClient client = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));

        //Create Agent
        ChatClientAgent agent = client.GetChatClient("gpt-4.1-nano").CreateAIAgent();

        Output.Title("Normal Call");
        AgentRunResponse response = await agent.RunAsync("What is the Capital of France?");
        Console.WriteLine(response);

        Output.Separator();

        Output.Title("Streaming Call");
        await foreach (AgentRunResponseUpdate update in agent.RunStreamingAsync("How to make soup?"))
        {
            Console.Write(update);
        }

        Output.Separator();

        Output.Title("Streaming Call (gathering all updates to a response at the end)");
        List<AgentRunResponseUpdate> updates = [];
        await foreach (AgentRunResponseUpdate update in agent.RunStreamingAsync("How to make soup?"))
        {
            updates.Add(update);
            Console.Write(update);
        }

        AgentRunResponse collectedResponse = updates.ToAgentRunResponse();
        //Use to the usage, and other return data...
        Console.WriteLine(collectedResponse.Usage!.OutputTokenCount);
    }
}