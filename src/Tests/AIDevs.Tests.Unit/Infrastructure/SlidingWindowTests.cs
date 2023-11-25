using AIDevs.Shared.Infrastructure.Embedding;
using AIDevs.Shared.Infrastructure.Whisper;
using AIDevs.Tests.Unit.Exercises.UnknowNews;
using Azure.AI.OpenAI;
using Grpc.Core;
using IdGen;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AIDevs.Tests.Unit.Infrastructure
{
    public class SlidingWindowTests
    {
        private const string CHAT_COMPLETIONS_MODEL_NAME = "gpt-3.5-turbo-1106";
        private const string _qdrant_collection_name = "transcription";

        private readonly QdrantClient _qdrantClient;
        private readonly TextEmbeddings _textEmbeddings;
        private readonly IdGenerator _idGen;
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly OpenAIClient _openAiClient;

        public SlidingWindowTests(ITestOutputHelper testOutputHelper)
        {
            _qdrantClient = new QdrantClient("localhost", port: 6334);
            _textEmbeddings = new TextEmbeddings(new HttpClient()
            {
                BaseAddress = new System.Uri("http://127.0.0.1:8080/"),
            });
            _idGen = new IdGen.IdGenerator(0);
            _testOutputHelper = testOutputHelper;
            _openAiClient = AzureOpenAiClientFactory.Create();
        }

        [Fact]
        public async Task Should_Create_Collection()
        {

            try
            {
                await _qdrantClient.CreateCollectionAsync(_qdrant_collection_name, new Qdrant.Client.Grpc.VectorParams()
                {
                    Size = 768,
                    Distance = Qdrant.Client.Grpc.Distance.Cosine,
                    OnDisk = true
                });
            }
            catch (RpcException ex)
            {
                ex.StatusCode.Should().Be(StatusCode.InvalidArgument);
            }

            var collectionInfo = await _qdrantClient.GetCollectionInfoAsync(_qdrant_collection_name);

            collectionInfo.Should().NotBeNull();
        }

        [Fact]
        public async Task Should_Create_Embedding_And_Save_In_Database()
        {
            // Arrange
            var windowWidth = TimeSpan.FromMinutes(3);

            var json = File.ReadAllText(@"D:\Repos\transcription.json");
            var transcription = JsonSerializer.Deserialize<List<TranscriptionSegment>>(json);

            var windows = transcription.ToTranscriptionWindows(windowWidth);

            // Act
            using var semaphore = new TimeSpanSemaphore(100, TimeSpan.FromSeconds(10));
            await Parallel.ForEachAsync(windows, async (window, token) =>
            {
                var embedding = await semaphore.Run(() => _textEmbeddings.GetEmbeddingsAsync(new(window.Content, true)));

                var pointStruct = new PointStruct()
                {
                    Id = (ulong)_idGen.CreateId(),
                    Vectors = embedding,
                    Payload =
                    {
                        ["from"] = window.TimeStamp.From.Ticks,
                        ["to"] = window.TimeStamp.To.Ticks,
                        ["content"] = window.Content
                    }
                };

                await _qdrantClient.UpsertAsync(_qdrant_collection_name, new[] { pointStruct }, cancellationToken: token);
            });

            var collectionInfo = await _qdrantClient.GetCollectionInfoAsync(_qdrant_collection_name);

            // Assert
            collectionInfo.VectorsCount.Should().Be((ulong)windows.Count);
        }

        [Fact]
        public async Task Should_List_Main_Facts_About_Podcast()
        {
            // Arrange
            var windowWidth = TimeSpan.FromMinutes(30);

            var json = File.ReadAllText(@"D:\Repos\transcription.json");
            var transcription = JsonSerializer.Deserialize<List<TranscriptionSegment>>(json);



            var windows = transcription.Chunk(transcription.Count / 5).Select(x => new TranscriptionWindow(x));

            var systemPrompt = @"
Podcast Fragment Analysis:


1. Key Insights:

Guest shares unique perspectives on the discussed subject.
Unusual or surprising facts are highlighted.

2. Engaging Anecdotes:

Memorable stories or personal experiences shared by the guest.
Moments that add a human touch to the conversation.

3. Controversial Points:

Identification of any controversial statements or opinions.
Examination of contrasting viewpoints presented.

4. Humorous Moments:

Instances of humor or witty remarks during the conversation.
Any lighthearted moments that stand out.

5. Noteworthy Quotes:

Compilation of impactful quotes from the guest or host.
Emphasis on insightful or thought-provoking statements.

6. Emerging Trends:

Discussion of any trends or predictions mentioned in the podcast.
Exploration of future implications or developments.

7. Interactive Elements:

Mention of audience engagement, such as Q&A sessions.
Opportunities for listeners to participate in discussions.

8. Calls to Action:

Any calls to action suggested by the guest or host.
Encouragement for listeners to explore further resources.


If a piece of text does not mention a point, do not mention it.


Format output as JSON Object
{
  ""Key Insights"" : [""Point one"", ""Point two""],
  ""Engaging Anecdotes"" : [""Point one"", ""Point two""],
  ""Controversial Points"" : [""Point one"", ""Point two""],
  ""Humorous Moments"" : [""Point one"", ""Point two""],
  ""Noteworthy Quotes"" : [""Point one"", ""Point two""],
  ""Emerging Trends"" : [""Point one"", ""Point two""],
  ""Interactive Elements"" : [""Point one"", ""Point two""],
  ""Calls to Action"" : [""Point one"", ""Point two""]
}
";

            foreach (var window in windows)
            {
                var completionsOptions = new ChatCompletionsOptions();
                completionsOptions.Messages.Add(new(ChatRole.System, systemPrompt));
                completionsOptions.Messages.Add(new(ChatRole.User, window.Content));

                var completionResult = await _openAiClient.GetChatCompletionsAsync(CHAT_COMPLETIONS_MODEL_NAME, completionsOptions);
                var result = completionResult.Value.Choices.First().Message.Content;

                var points = JsonSerializer.Deserialize<Dictionary<string, string[]>>(result)["Key Insights"];

                _testOutputHelper.WriteLine($"From: {window.TimeStamp.From}, To: {window.TimeStamp.To}");
                foreach (var point in points)
                {
                    _testOutputHelper.WriteLine(point);
                }
            }
        }

        [Fact]
        public async Task Should_Find_Part_Of_The_Video_With_Answer_To_Question()
        {
            var questions = new string[]
            {
                "Discussion of artificial intelligence and its impact on society",
                "Exploration of the evolution and potential of AI technology",
                "Technology's evolution reflects the complexity in the biological world",
                "General artificial intelligence may not necessarily have internal states or consciousness",
                "Discussion of the value of conversational technology and its ease of use for personalized experiences.",
                "Exploration of the impact of technology on relationships and emotions, particularly in the context of artificial intelligence.",
                "The guest discusses the influence of individuals, including top decision-makers like Elon Musk, on the development and use of technology.",
                "The conversation highlights the fascination and honesty behind the creation of new technologies, like the GPT chat, and the element of experimentation involved in engaging with them.",
                "The use of AI language models as a substitute for therapeutic services due to limited access to real therapists",
                "The impact of COVID-19 on the value of human contact and emotional relationships"
            };

            var questionsEmbeddings = new List<Embedding<string>>(questions.Length);

            using var semaphore = new TimeSpanSemaphore(100, TimeSpan.FromSeconds(10));
            await Parallel.ForEachAsync(questions, async (question, _) =>
            {
                var embedding = await semaphore.Run(() => _textEmbeddings.GetEmbeddingsAsync(new(question, true)));

                questionsEmbeddings.Add(Embedding<string>.Create(question, embedding));
            });

            foreach (var question in questionsEmbeddings)
            {
                var answers = await _qdrantClient.SearchAsync(_qdrant_collection_name, question.Vector, limit: 1);

                _testOutputHelper.WriteLine($"Topic: {question.Input}");

                foreach (var answer in answers)
                {
                    var from = (int)TimeSpan.FromTicks(answer.Payload["from"].IntegerValue).TotalSeconds;
                    var to = (int)TimeSpan.FromTicks(answer.Payload["to"].IntegerValue).TotalSeconds;
                    var content = answer.Payload["content"].StringValue;

                    _testOutputHelper.WriteLine($"---------------------------------------------------------------------------------------------------------------");
                    _testOutputHelper.WriteLine($"  Score: {answer.Score} Link: https://www.youtube.com/embed/YOOscNI0Jaw?autoplay=1&start={from}&end={to}&Rel=0");
                    _testOutputHelper.WriteLine($"  Content: {content}");
                    _testOutputHelper.WriteLine($"---------------------------------------------------------------------------------------------------------------");
                }



            }
        }
    }
}
