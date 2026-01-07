using Azure.AI.OpenAI;
using Microsoft.Agents.AI;
using OpenAI;
using OpenAI.Chat;

namespace Samples.SampleUtilities.Agents;

public static class AgentHelper
{
    public static ChatClientAgent GetAgent(string instructions, Provider? providerToUse = null, string? modelToUse = null)
    {
        providerToUse ??= ConfigurationDefaults.DefaultProviderToUse;
        modelToUse ??= ConfigurationDefaults.DefaultModelToUse;
        switch (providerToUse)
        {
            case Provider.AzureOpenAIApiKey:
                AzureOpenAIClient azureOpenAIClient = Providers.AzureOpenAI.GetClientFromApiKey();
                return azureOpenAIClient.GetChatClient(modelToUse).CreateAIAgent(instructions: instructions);
            case Provider.OpenAI:
                OpenAIClient openAIClient = Providers.OpenAI.GetClient();
                return openAIClient.GetChatClient(modelToUse).CreateAIAgent(instructions: instructions);
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}