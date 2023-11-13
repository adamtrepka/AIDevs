using AIDevs.Shared.Infrastructure.Embedding;
using AIDevs.Tests.Unit.Exercises.UnknowNews;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Json;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AIDevs.Tests.Unit.Infrastructure
{
    public class TimespanSemaphoreTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public TimespanSemaphoreTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task Should_Generate_Embeddings()
        {
            // Arrange
            var textEmbeddings = new TextEmbeddings(new HttpClient()
            {
                BaseAddress = new Uri("http://127.0.0.1:8080/"),
            });

            var httpClient = new HttpClient();
            var httpResponse = await httpClient.GetAsync("https://unknow.news/archiwum.json");
            var archive = await httpResponse.Content.ReadFromJsonAsync<List<Article>>();

            var embedings = new List<Embedding<Article>>(archive.Count);

            // Act

            using var semaphore = new TimeSpanSemaphore(100, TimeSpan.FromSeconds(10));

            Parallel.ForEach(archive, async article =>
            {
                var embedding = await semaphore.Run(() => textEmbeddings.GetEmbeddingsAsync(new(article.Info, true)));

                embedings.Add(Embedding<Article>.Create(article, embedding));
                Debug.WriteLine($"Embeddings count: {embedings.Count}/{archive.Count}");
            });

            var questionEmbedding = await textEmbeddings.GetEmbeddingsAsync(new("Raspberry",true));

            var answer = embedings
                .OrderByDescending(embedding => Extensions.CosineSimilarity(questionEmbedding, embedding.Vector))
                .Take(4);
        }
    }
}
