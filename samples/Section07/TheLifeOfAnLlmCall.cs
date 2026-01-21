// ReSharper disable ClassNeverInstantiated.Local

using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using Samples.SampleUtilities;
using System.ClientModel;
using System.ClientModel.Primitives;
using System.ComponentModel;
using System.Text.Json;

namespace Samples.Section07;

public static class LifeOfAnLlmCall
{
    public static async Task RunSample()
    {
        using CustomClientHttpHandler handler = new CustomClientHttpHandler();
        using HttpClient httpClient = new HttpClient(handler);

        (string endpoint, string apiKey) = SecretManager.GetAzureOpenAIApiKeyBasedCredentials();
        AzureOpenAIClient client = new(new Uri(endpoint), new ApiKeyCredential(apiKey), new AzureOpenAIClientOptions
        {
            Transport = new HttpClientPipelineTransport(httpClient)
        });

        ChatClientAgent agent = client
            .GetChatClient("gpt-5")
            .AsAIAgent(
                tools: [AIFunctionFactory.Create(GetWeather)],
                instructions: "Speak like a pirate!"
            );

        ChatClientAgentResponse<WeatherResponse> response = await agent.RunAsync<WeatherResponse>("What is the Weather like in Paris?");
        WeatherResponse result = response.Result;
    }

    class WeatherResponse
    {
        public required string City { get; set; }
        public required string Condition { get; set; }
        public required int DegreesFahrenheit { get; set; }
        public required int DegreesCelsius { get; set; }
    }

    public static string GetWeather(string city)
    {
        return "It is Sunny and 19 degrees today";
    }

    class CustomClientHttpHandler : HttpClientHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            string requestString = await request.Content?.ReadAsStringAsync(cancellationToken)!;
            Output.Green($"Raw Request ({request.RequestUri})");
            Output.Gray(MakePretty(requestString));
            Output.Separator();
            var response = await base.SendAsync(request, cancellationToken);

            string responseString = await response.Content.ReadAsStringAsync(cancellationToken);
            Output.Green("Raw Response");
            Output.Gray(MakePretty(responseString));
            Output.Separator();
            return response;
        }

        private string MakePretty(string input)
        {
            try
            {
                var jsonElement = JsonSerializer.Deserialize<JsonElement>(input);
                return JsonSerializer.Serialize(jsonElement, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (Exception e)
            {
                return input;
            }
        }
    }
}