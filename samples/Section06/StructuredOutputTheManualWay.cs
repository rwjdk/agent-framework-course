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

public static class StructuredOutputTheManualWay
{
    public static async Task RunSample()
    {
        //Create Raw Connection
        (string endpoint, string apiKey) = SecretManager.GetAzureOpenAIApiKeyBasedCredentials();
        AzureOpenAIClient client = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));

        //Create Agent
        AIAgent agent = client
            .GetChatClient("gpt-4.1-nano")
            .AsAIAgent(instructions: "You are a Movie Expert")
            .AsBuilder()
            //middleware goes here
            .Build();

        string question = "List the top 3 best movies according to IMDB";

        JsonSerializerOptions jsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
            Converters = { new JsonStringEnumConverter() }
        };

        ChatResponseFormatJson chatResponseFormatJson = ChatResponseFormat.ForJsonSchema<MovieResult>(jsonSerializerOptions);

        Output.Title("Structured Output Call");
        AgentResponse response = await agent.RunAsync(question, options: new ChatClientAgentRunOptions
        {
            ChatOptions = new ChatOptions
            {
                ResponseFormat = chatResponseFormatJson
            }
        });

        Output.Gray("response.Result = .NET Object you can format as you see fit");

        MovieResult movieResult = response.Deserialize<MovieResult>(jsonSerializerOptions);
        foreach (Movie movie in movieResult.Movies)
        {
            Console.WriteLine($"- Title: {movie.Title} - " +
                              $"Director: {movie.Director} - " +
                              $"Year: {movie.YearOfRelease} - " +
                              $"Score: {movie.ImdbScore}");
        }

        Console.WriteLine();
        Output.Gray("response.Text = Raw JSON");
        Console.WriteLine(response.Text);
    }

    private class MovieResult
    {
        public required List<Movie> Movies { get; set; }
    }

    private class Movie
    {
        public required string Title { get; set; }
        public required string Director { get; set; }
        public required int YearOfRelease { get; set; }
        public required decimal ImdbScore { get; set; }
    }
}