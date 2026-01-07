using System.ClientModel;
using System.Diagnostics;
using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using OpenAI.Chat;
using Samples.SampleUtilities;

namespace Samples.Section03;

public static class TokenUsage
{
    public static async Task RunSample()
    {
        //Create Raw Connection
        (string endpoint, string apiKey) = SecretManager.GetAzureOpenAIApiKeyBasedCredentials();
        AzureOpenAIClient client = new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));

        await RunByModel(client, "gpt-4.1-nano");
        await RunByModel(client, "gpt-5-nano");
        await RunByModel(client, "gpt-5.2");
    }

    private static async Task RunByModel(AzureOpenAIClient client, string model)
    {
        Output.Gray($"Testing Model: {model} on Azure OpenAI");
        Console.WriteLine();

        //Create Agent
        ChatClientAgent agent = client.GetChatClient(model).CreateAIAgent();

        string? message = "What is the capital of France?";

        Output.Blue("Input:");
        Console.WriteLine(message);

        Stopwatch stopwatch = Stopwatch.StartNew();

        Console.WriteLine();

        AgentRunResponse response = await agent.RunAsync(message);
        long milliseconds = stopwatch.ElapsedMilliseconds;

        Output.Green("Output:");

        Console.WriteLine(response);
        Console.WriteLine();

        Output.Red("Usage:");

        if (response.Usage != null)
        {
            Console.WriteLine($"- Input Tokens: {response.Usage.InputTokenCount}");
            Console.WriteLine($"- Cached Tokens: {response.Usage.CachedInputTokenCount ?? 0}");
            Console.WriteLine($"- Output Tokens: {response.Usage.OutputTokenCount} " +
                              $"({response.Usage.ReasoningTokenCount ?? 0} being reasoning Tokens)");
        }

        Console.WriteLine();

        Output.Magenta("Time spent:");
        Console.WriteLine($"{milliseconds} milli-seconds");

        Output.Separator();
    }
}