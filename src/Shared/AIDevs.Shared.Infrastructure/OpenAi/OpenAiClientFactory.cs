namespace AIDevs.Shared.Infrastructure.OpenAi
{
    public static class OpenAiClientFactory
    {
        private static HttpClient _httpClient;

        static OpenAiClientFactory()
        {
            var apiKey = Environment.GetEnvironmentVariable(EnvironmentVariableNames.OPENAI_APIKEY_ENVIRONMENT_VARIABLE_NAME);
            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri("https://api.openai.com"),
            };
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
        }

        public static OpenAiClient Create() => new OpenAiClient(_httpClient);
    }
}
