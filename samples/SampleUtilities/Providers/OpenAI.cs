using OpenAI;

namespace Samples.SampleUtilities.Providers;

public class OpenAI
{
    public static OpenAIClient GetClient()
    {
        string apiKey = SecretManager.GetOpenAIApiKey();
        return new OpenAIClient(apiKey);
    }
}