using AIDevs.Tests.Unit.Infrastructure;
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
                var moderationResult = await OpenAiClient.GetModerationAsync(paragraph);
                var flaged = moderationResult?.results?.Any(x => x.flagged) ?? false;
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

            // Assert
        }
    }
}
