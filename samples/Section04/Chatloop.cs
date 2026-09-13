using Microsoft.Agents.AI;
using OpenAI;
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
        OpenAIClient client = new OpenAIClient(new ApiKeyCredential(apiKey), new OpenAIClientOptions
        {
            Endpoint = new Uri(endpoint + "/openai/v1")
        });

        //Create Agent
        ChatClientAgent agent = client.GetChatClient("gpt-4.1-nano").AsAIAgent();

        AgentSession session = await agent.CreateSessionAsync();

        while (true)
        {
            Console.Write("> ");
            string input = Console.ReadLine() ?? "";
            List<AgentResponseUpdate> updates = [];
            await foreach (AgentResponseUpdate update in agent.RunStreamingAsync(input, session))
            {
                updates.Add(update);
                Console.Write(update);
            }

            AgentResponse response = updates.ToAgentResponse();
            if (response.Usage != null)
            {
                Console.WriteLine();
                Output.Gray($"Tokens - In: {response.Usage.InputTokenCount} - Out: {response.Usage.OutputTokenCount}");
            }

            InMemoryChatHistoryProvider? historyProvider = agent.GetService<InMemoryChatHistoryProvider>();
            IList<ChatMessage> messagesForSession = historyProvider?.GetMessages(session) ?? [];

            Output.Separator();
        }
    }
}