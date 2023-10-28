using System.Text.Json;

namespace AIDevs.Tests.Unit.Exercises
{
    public class AiDevsExerciseTests : AiDevsExerciseBaseTests
    {
        public AiDevsExerciseTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        [Fact(DisplayName = "Exercise 01 - helloapi")]
        public async Task Should_Send_Cookie_Value()
        {
            // Arrange
            var token = await ExercisesClient.GetTokenAsync("helloapi");
            var task = await ExercisesClient.GetTaskAsync(token.Token);

            TestOutputHelper.WriteLine("Task message: {0}", task.Msg);

            // Act
            var answer = task.AdditionalData["cookie"].ToString();
            var result = await ExercisesClient.SendResponseAsync(token.Token, answer);

            // Assert
            result.Should().NotBeNull();
            result.Code.Should().Be(0);

            TestOutputHelper.WriteLine("Result message: {0}", result.Msg);
        }

        [Fact(DisplayName = "Exercise 02 - moderation")]
        public async Task Should_Send_Moderate_Paragraphs()
        {
            // Arrange
            var token = await ExercisesClient.GetTokenAsync("moderation");
            var task = await ExercisesClient.GetTaskAsync(token.Token);

            TestOutputHelper.WriteLine("Task message: {0}", task.Msg);

            // Act
            var paragraphs = task.AdditionalData["input"].Deserialize<string[]>();

            var answer = new List<int>();

            foreach(var paragraph in paragraphs)
            {
                var moderationResult = await OpenAiClient.ModerationsAsync(new Shared.Infrastructure.OpenAi.CreateModerationRequest()
                {
                    Input = paragraph
                });
                var flaged = moderationResult?.Results?.Any(x => x.Flagged) ?? false;
                answer.Add(flaged ? 1 : 0);
            }

            var result = await ExercisesClient.SendResponseAsync(token.Token, answer);

            // Assert
            result.Should().NotBeNull();
            result.Code.Should().Be(0);

            TestOutputHelper.WriteLine("Result message: {0}", result.Msg);
        }

        [Fact(DisplayName = "Exercise 03 - blogger")]
        public async Task Should_Send_Write_Blogpost()
        {
            // Arrange
            var token = await ExercisesClient.GetTokenAsync("blogger");
            var task = await ExercisesClient.GetTaskAsync(token.Token);

            TestOutputHelper.WriteLine("Task message: {0}", task.Msg);

            // Act
            var chapters = task.AdditionalData["blog"].Deserialize<string[]>();

            var answer = new List<string>();

            for (int i = 0; i < chapters.Length; i++)
            {
                string? chapter = chapters[i];
                var completionResult = await OpenAiClient.ChatCompletionsAsync(new CreateChatCompletionRequest
                {
                    Model = "gpt-3.5-turbo",
                    Messages = new ChatCompletionRequestMessage[]
                    {
                        new ChatCompletionRequestMessage
                        {
                            Role = ChatCompletionRequestMessageRole.system,
                            Content = "You are the author of a blog. The blog is written in Polish. You prepare your blog post divided into chapters."
                        },
                        new ChatCompletionRequestMessage
                        {
                            Role = ChatCompletionRequestMessageRole.user,
                            Content = $"Prepare a chapter of your blog post entitled: '{i} - {chapter}'."
                        }
                    },
                    Max_tokens = 200
                });

                var text = completionResult.Choices.Select(x => x.Message.Content).FirstOrDefault();
                answer.Add(text);
            }

            var result = await ExercisesClient.SendResponseAsync(token.Token, answer);

            // Assert
            TestOutputHelper.WriteLine("Result message: {0}", result.Msg);

            result.Should().NotBeNull();
            result.Code.Should().Be(0);

        }
    }
}
