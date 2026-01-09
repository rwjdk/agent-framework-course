// ReSharper disable ClassNeverInstantiated.Local

using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using Samples.SampleUtilities;
using System.ClientModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using ChatResponseFormat = Microsoft.Extensions.AI.ChatResponseFormat;

namespace Samples.Section06;

public static class StructuredOutputLimitations
{
    public static async Task RunSample()
    {
        //Create Raw Connection
        (string endpoint, string apiKey) = SecretManager.GetAzureOpenAIApiKeyBasedCredentials();
        AzureOpenAIClient client = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));

        string question = "List the top 3 best movies according to IMDB";

        Output.Title("Structured Output Call (Nice way)");

        ChatClientAgent niceWayAgent = client.GetChatClient("gpt-4.1-nano").CreateAIAgent(instructions: "You are a Movie Expert");

        ChatClientAgentRunResponse<List<Movie>> clientAgentRunResponse = await niceWayAgent.RunAsync<List<Movie>>(question);
        List<Movie> movies = clientAgentRunResponse.Result;
        foreach (Movie movie in movies)
        {
            Console.WriteLine($"- Title: {movie.Title} - " +
                              $"Director: {movie.Director} - " +
                              $"Year: {movie.YearOfRelease} - " +
                              $"Score: {movie.ImdbScore}");
        }


        AIAgent cumbersomeWayAgent = client
            .GetChatClient("gpt-4.1-nano")
            .CreateAIAgent(instructions: "You are a Movie Expert")
            .AsBuilder()
            //middleware goes here
            .Build();

        JsonSerializerOptions jsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            Converters = { new JsonStringEnumConverter() }
        };

        ChatResponseFormatJson chatResponseFormatJson = ChatResponseFormat.ForJsonSchema<List<Movie>>(jsonSerializerOptions);

        Output.Title("Structured Output Call (Manual way)");

        Output.Red("NOTE: THE CALL TO THE LLM HERE WILL FAIL, AND THAT IS 'ON PURPOSE' FOR THIS SAMPLE. THE LECTURE ON STRUCTURED OUTPUT LIMITATIONS EXPLAINS WHY");

        AgentRunResponse response = await cumbersomeWayAgent.RunAsync(question, options: new ChatClientAgentRunOptions
        {
            ChatOptions = new ChatOptions
            {
                ResponseFormat = chatResponseFormatJson
            }
        });

        movies = response.Deserialize<List<Movie>>(jsonSerializerOptions);
        foreach (Movie movie in movies)
        {
            Console.WriteLine($"- Title: {movie.Title} - " +
                              $"Director: {movie.Director} - " +
                              $"Year: {movie.YearOfRelease} - " +
                              $"Score: {movie.ImdbScore}");
        }
    }

    private class Movie
    {
        public required string Title { get; set; }
        public required string Director { get; set; }
        public required int YearOfRelease { get; set; }
        public required decimal ImdbScore { get; set; }
    }
}