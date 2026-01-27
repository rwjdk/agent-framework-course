// ReSharper disable ClassNeverInstantiated.Local

using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.SqliteVec;
using OpenAI.Chat;
using Samples.SampleUtilities;
using System.ClientModel;
using System.Text;
using static Samples.Section08.IngestDataIntoVectorStore;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

namespace Samples.Section08;

public static class SearchAndUseVectorStore
{
    public static async Task RunSample()
    {
        //Create Raw Connection
        (string endpoint, string apiKey) = SecretManager.GetAzureOpenAIApiKeyBasedCredentials();
        AzureOpenAIClient client = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));

        //Define Embedding Generator
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator = client
            .GetEmbeddingClient("text-embedding-3-small")
            .AsIEmbeddingGenerator();

        //Define Vector Store
        string connectionString = $"Data Source={Path.GetTempPath()}\\af-course-vector-store.db";
        VectorStore vectorStore = new Microsoft.SemanticKernel.Connectors.SqliteVec.SqliteVectorStore(connectionString, new SqliteVectorStoreOptions
        {
            EmbeddingGenerator = embeddingGenerator
        });

        //Get Vector Store Collection (so we can search against it)
        VectorStoreCollection<Guid, KnowledgeBaseVectorRecord> vectorStoreCollection = vectorStore.GetCollection<Guid, KnowledgeBaseVectorRecord>("knowledge_base");

        //Create Agent
        ChatClientAgent agent = client
            .GetChatClient("gpt-4.1-nano")
            .AsAIAgent(instructions: "You are an expert in the companies Internal Knowledge Base");

        AgentSession session = await agent.GetNewSessionAsync();;

        while (true)
        {
            Console.Write("> ");
            string input = Console.ReadLine() ?? "";

            StringBuilder mostSimilarKnowledge = new StringBuilder();
            await foreach (VectorSearchResult<KnowledgeBaseVectorRecord> searchResult in vectorStoreCollection.SearchAsync(input, 3))
            {
                string searchResultAsQAndA = $"Q: {searchResult.Record.Question} - A: {searchResult.Record.Answer}";
                Output.Gray($"Search result [Score: {searchResult.Score}] {searchResultAsQAndA}");
                mostSimilarKnowledge.AppendLine(searchResultAsQAndA);
            }

            List<ChatMessage> messagesToSend =
            [
                new ChatMessage(ChatRole.User, "Here is the most relevant Knowledge base information: " + mostSimilarKnowledge),
                new ChatMessage(ChatRole.User, input)
            ];

            AgentResponse response = await agent.RunAsync(messagesToSend, session);
            {
                Output.Yellow("Final Answer after Search + LLM");
                Console.WriteLine(response);
            }

            Output.Separator();
        }
    }
}