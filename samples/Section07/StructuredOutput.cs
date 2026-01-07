// ReSharper disable ClassNeverInstantiated.Local

using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Samples.SampleUtilities;
using System.ClientModel;
using OpenAI.Chat;

namespace Samples.Section07;

public static class StructuredOutput
{
    public static async Task RunSample()
    {
        //Create Raw Connection
        (string endpoint, string apiKey) = SecretManager.GetAzureOpenAIApiKeyBasedCredentials();
        AzureOpenAIClient client = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));

        //Create Agent
        ChatClientAgent agent = client.GetChatClient("gpt-4.1-nano").CreateAIAgent(instructions: "You are a Movie Expert");

        string question = "List the top 3 best movies according to IMDB";

        Output.Title("Normal Call");
        AgentRunResponse normalResponse = await agent.RunAsync(question);
        Output.Gray("normalResponse.Text = A variable format decided by the AI");
        Console.WriteLine(normalResponse);
        /* Issue:
         * While the answer will be correct, the response will slightly differ for call to call
         * - Sometimes Director or the movie is mentioned, sometimes not...
         * - Sometimes Markdown is used, sometimes not...
         * - Text before and after list changes
         */

        Output.Separator();

        Output.Title("Structured Output Call");
        ChatClientAgentRunResponse<MovieResult> response = await agent.RunAsync<MovieResult>(question);

        Output.Gray("response.Result = .NET Object you can format as you see fit");

        MovieResult movieResult = response.Result;
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