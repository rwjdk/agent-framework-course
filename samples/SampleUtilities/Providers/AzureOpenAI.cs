using System.ClientModel;
using Azure.AI.OpenAI;
using Azure.Identity;

namespace Samples.SampleUtilities.Providers;

public class AzureOpenAI
{
    public static AzureOpenAIClient GetClientFromApiKey()
    {
        (string endpoint, string apiKey) = SecretManager.GetAzureOpenAIApiKeyBasedCredentials();
        return new AzureOpenAIClient(new Uri(endpoint), new ApiKeyCredential(apiKey));
    }

    public static AzureOpenAIClient GetClientFromRoleBasedAccessControl()
    {
        string endpoint = SecretManager.GetAzureOpenAIRoleBaseAccessControl();
        return new AzureOpenAIClient(new Uri(endpoint), new AzureCliCredential());
    }
}