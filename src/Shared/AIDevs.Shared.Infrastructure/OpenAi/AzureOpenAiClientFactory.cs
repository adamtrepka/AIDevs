using Azure.AI.OpenAI;

namespace AIDevs.Shared.Infrastructure.OpenAi
{
    public static class AzureOpenAiClientFactory
    {
        public static OpenAIClient Create()
        {
            var apiKey = Environment.GetEnvironmentVariable(EnvironmentVariableNames.OPENAI_APIKEY_ENVIRONMENT_VARIABLE_NAME);
            return new(apiKey);
        }
    }
}
