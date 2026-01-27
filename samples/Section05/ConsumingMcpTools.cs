using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using Samples.SampleUtilities;
using System.ClientModel;
using System.Text;
using ModelContextProtocol.Client;

namespace Samples.Section05;

public static class ConsumingMcpTools
{
    public static async Task RunSample()
    {
        //Create Raw Connection
        (string endpoint, string apiKey) = SecretManager.GetAzureOpenAIApiKeyBasedCredentials();
        AzureOpenAIClient client = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));

        await using McpClient mcpClient = await McpClient.CreateAsync(new HttpClientTransport(new HttpClientTransportOptions
        {
            Endpoint = new Uri("https://learn.microsoft.com/api/mcp"),
            TransportMode = HttpTransportMode.StreamableHttp
        }));
        IList<McpClientTool> mcpTools = await mcpClient.ListToolsAsync();

        //Create Agent
        ChatClientAgent agent = client
            .GetChatClient("gpt-4.1-nano")
            .AsAIAgent(
                instructions: "You are an Expert in the C# version of Microsoft Agent Framework " +
                              "(use tools to find your knowledge) " +
                              "and assume Azure OpenAI with API Key is used",
                tools: mcpTools.Cast<AITool>().ToList()
            );

        AgentSession session = await agent.GetNewSessionAsync();;

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