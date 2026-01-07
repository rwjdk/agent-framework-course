using Microsoft.Extensions.Configuration;

namespace Samples.SampleUtilities;

public class SecretManager
{
    public static (string endpoint, string apiKey) GetAzureOpenAIApiKeyBasedCredentials()
    {
        IConfigurationRoot configuration = GetConfiguration();
        string endpoint = configuration["AzureEndpoint"] ?? ThrowMissingSecretException("AzureEndpoint");
        string apiKey = configuration["AzureApiKey"] ?? ThrowMissingSecretException("AzureApiKey");
        return (endpoint, apiKey);
    }

    public static string GetAzureOpenAIRoleBaseAccessControl()
    {
        IConfigurationRoot configuration = GetConfiguration();
        string endpoint = configuration["AzureEndpoint"] ?? ThrowMissingSecretException("AzureEndpoint");
        return endpoint;
    }

    public static string GetOpenAIApiKey()
    {
        IConfigurationRoot configuration = GetConfiguration();
        string apiKey = configuration["OpenAIApiKey"] ?? ThrowMissingSecretException("OpenAIApiKey");
        return apiKey;
    }

    private static string ThrowMissingSecretException(string variable)
    {
        throw new Exception($"Secret '{variable}' is missing; Add it to UserSecrets or Environment Variables");
    }

    private static IConfigurationRoot GetConfiguration()
    {
        return new ConfigurationBuilder().AddUserSecrets<SecretManager>().Build();
    }
}