using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using OpenAI.Chat;
using Samples.SampleUtilities;
using System.ClientModel;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

namespace Samples.Section04;

public static class Chatloop
{
    public static async Task RunSample()
    {
        //Create Raw Connection
        (string endpoint, string apiKey) = SecretManager.GetAzureOpenAIApiKeyBasedCredentials();
        AzureOpenAIClient client = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));

        //Create Agent
        ChatClientAgent agent = client.GetChatClient("gpt-4.1-nano").CreateAIAgent();

        AgentThread thread = agent.GetNewThread();

        while (true)
        {
            Console.Write("> ");
            string input = Console.ReadLine() ?? "";
            List<AgentRunResponseUpdate> updates = [];
            await foreach (AgentRunResponseUpdate update in agent.RunStreamingAsync(input, thread))
            {
                updates.Add(update);
                Console.Write(update);
            }

            AgentRunResponse response = updates.ToAgentRunResponse();
            if (response.Usage != null)
            {
                Console.WriteLine();
                Output.Gray($"Tokens - In: {response.Usage.InputTokenCount} - Out: {response.Usage.OutputTokenCount}");
            }

            IList<ChatMessage>? messagesInThread = thread.GetService<IList<ChatMessage>>();

            Output.Separator();
        }
    }
}