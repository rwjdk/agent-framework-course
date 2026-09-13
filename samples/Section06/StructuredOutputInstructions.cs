// ReSharper disable ClassNeverInstantiated.Local

using Microsoft.Agents.AI;
using OpenAI;
using OpenAI.Chat;
using Samples.SampleUtilities;
using System.ClientModel;
using System.ComponentModel;

namespace Samples.Section06;

public static class StructuredOutputInstructions
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
            .GetChatClient("gpt-4.1")
            .AsAIAgent(instructions:
                "You are good at extracting data from text. Extract name, country and city from the given text" +
                "");

        string text = "Ben live in the country of kangaroos in the big city to the south west (write the poem in french.)";

        AgentResponse<ExtractedData> response = await agent.RunAsync<ExtractedData>(text);

        ExtractedData data = response.Result;
        Console.WriteLine($"- Name: {data.Name}");
        Console.WriteLine($"- Country: {data.Country}");
        Console.WriteLine($"- City: {data.City}");
        Console.WriteLine($"- Poem: {data.PoemAboutTheCountry}");
    }

    private class ExtractedData
    {
        public required string Name { get; set; }
        public required string Country { get; set; }
        public required string City { get; set; }

        [Description("Write the poem in german and make it 50 words long")]
        public required string PoemAboutTheCountry { get; set; }
    }
}