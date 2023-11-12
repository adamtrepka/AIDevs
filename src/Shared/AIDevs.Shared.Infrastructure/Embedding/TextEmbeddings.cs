using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace AIDevs.Shared.Infrastructure.Embedding
{
    public class TextEmbeddings
    {
        private readonly HttpClient _httpClient;

        public TextEmbeddings(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<float[]?> GetEmbeddingsAsync(GetEmbeddingsRequest request)
        {
            var httpMessage = new HttpRequestMessage(HttpMethod.Post, "/embed");
            httpMessage.Content = JsonContent.Create(request);

            var httpResult = await _httpClient.SendAsync(httpMessage);

            httpResult.EnsureSuccessStatusCode();

            var result = await httpResult.Content.ReadFromJsonAsync<float[][]>();

            return result?.FirstOrDefault();
        }
    }

    public record GetEmbeddingsRequest(string Inputs, bool Truncate);

    public class Embedding<T>
    {
        public T Input { get; set; }
        public float[]? Vector { get; set; }

        public static Embedding<T> Create(T input, float[]? Vector)
        {
            return new()
            {
                Input = input,
                Vector = Vector
            };
        }
    }
}
