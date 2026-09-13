using Microsoft.Agents.AI;
using OpenAI;
using OpenAI.Chat;
using Samples.SampleUtilities;
using System.ClientModel;
using System.Text;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

namespace Samples.Section04;

public static class Instructions
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
        ChatClientAgent agent = client
            .GetChatClient("gpt-4.1-nano")
            .AsAIAgent(instructions: "Speak like a pirate");

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
}