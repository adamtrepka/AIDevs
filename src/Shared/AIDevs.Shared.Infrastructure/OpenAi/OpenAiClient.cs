using AIDevs.Shared.Abstraction.OpenAi.Moderation;
using System.Net.Http.Json;

namespace AIDevs.Shared.Infrastructure.OpenAi
{
    public class OpenAiClient
    {
        private readonly HttpClient _httpClient;

        public OpenAiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async ValueTask<ModerationResult> GetModerationAsync(string input)
        {
            var request = new ModerationRequest(input);

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/v1/moderations");
            httpRequest.Content = JsonContent.Create(request);

            var httpResult = await _httpClient.SendAsync(httpRequest);

            return await httpResult.Content.ReadFromJsonAsync<ModerationResult>();
        }
    }
}
