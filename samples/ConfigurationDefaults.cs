namespace Samples;

public static class ConfigurationDefaults
{
    public static Provider DefaultProviderToUse = Provider.AzureOpenAIApiKey;
    public static string DefaultModelToUse = "gpt-4.1-nano"; //Note: For Azure, you need to deploy this model
}

public enum Provider
{
    AzureOpenAIApiKey, //Require an Endpoint ('https://<name>.services.ai.azure.com' or 'https://<name>.openai.azure.com') and API Key from ai.azure.com
    AzureOpenAIRoleBasedAccessControl, //Require an Endpoint ('https://<name>.services.ai.azure.com' or 'https://<name>.openai.azure.com') and AzureCLI installed and configured
    OpenAI, //Require an OpenAI API Account from https://platform.openai.com/
}