using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
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
        AzureOpenAIClient client = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));

        //Create Agent
        ChatClientAgent agent = client
            .GetChatClient("gpt-4.1-nano")
            .CreateAIAgent(instructions: "Speak like a pirate");

        AgentThread thread = agent.GetNewThread();

        Console.OutputEncoding = Encoding.UTF8;
        while (true)
        {
            Console.Write("> ");
            string input = Console.ReadLine() ?? "";
            AgentRunResponse response = await agent.RunAsync(input, thread);
            {
                Console.WriteLine(response);
            }

            Output.Separator();
        }
    }
}