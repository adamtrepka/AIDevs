using AIDevs.Shared.Abstraction.Api.Answer;
using AIDevs.Shared.Abstraction.Api.Task;
using AIDevs.Shared.Abstraction.Api.Token;
using System.Net.Http.Json;
using System.Text.Json;

namespace AIDevs.Shared.Infrastructure.Excercises
{
    public class AiDevsExercisesClient
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        public AiDevsExercisesClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async ValueTask<TokenResponse> GetTokenAsync(string taskName)
        {
            var apiKey = Environment.GetEnvironmentVariable(EnvironmentVariableNames.AIDEVS_APIKEY_ENVIRONMENT_VARIABLE_NAME)
                ?? throw new ArgumentNullException(EnvironmentVariableNames.AIDEVS_APIKEY_ENVIRONMENT_VARIABLE_NAME);
            var request = new TokenRequest(apiKey);

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/token/{taskName}");
            httpRequest.Content = JsonContent.Create(request, options: _jsonSerializerOptions);

            var httpResponse = await _httpClient.SendAsync(httpRequest);

            return await httpResponse.Content.ReadFromJsonAsync<TokenResponse>();
        }

        public async ValueTask<TaskResponse> GetTaskAsync(string token)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Get, $"task/{token}");
            var httpResponse = await _httpClient.SendAsync(httpRequest);

            return await httpResponse.Content.ReadFromJsonAsync<TaskResponse>();
        }

        public async ValueTask<TaskResponse> PostTaskAsync(string token, IDictionary<string,string> formData)
        {
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"task/{token}");

            httpRequest.Content = new FormUrlEncodedContent(formData);

            var httpResponse = await _httpClient.SendAsync(httpRequest);

            return await httpResponse.Content.ReadFromJsonAsync<TaskResponse>();
        }

        public async ValueTask<AnswerResponse> SendResponseAsync<TAnswer>(string token, TAnswer answer)
        {
            var request = new AnswerRequest<TAnswer>(answer);

            var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"answer/{token}");

            httpRequest.Content = JsonContent.Create(request, options: _jsonSerializerOptions);
   
            var httpResponse = await _httpClient.SendAsync(httpRequest);

            return await httpResponse.Content.ReadFromJsonAsync<AnswerResponse>();

        }
    }
}