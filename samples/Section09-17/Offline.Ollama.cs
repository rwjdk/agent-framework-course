using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OllamaSharp;
namespace Samples.Section09_17;

public static class OfflineOllama
{
    public static async Task RunSample()
    {
        string model = "llama3.2:latest";
        string ollamaEndpoint = "http://localhost:11434";
        IChatClient client = new OllamaApiClient(ollamaEndpoint, model);
        AIAgent agent = new ChatClientAgent(client);

        AgentResponse response = await agent.RunAsync("What is the Capital of Sweden?");
        Console.WriteLine(response);

        Console.WriteLine("---");

        await foreach (AgentResponseUpdate update in agent.RunStreamingAsync("How to make soup?"))
        {
            Console.Write(update);
        }
    }
}
