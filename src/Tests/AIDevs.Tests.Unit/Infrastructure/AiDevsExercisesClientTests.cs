
namespace AIDevs.Tests.Unit.Infrastructure
{
    public class AiDevsExercisesClientTests
    {
        private readonly ITestOutputHelper _testOutputHelper;

        public AiDevsExercisesClientTests(ITestOutputHelper testOutputHelper)
        {
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task GetTokenAsync_Should_Return_Token()
        {
            // Arrange
            var client = AiDevsExercisesClientFactory.Create();

            // Act
            var token = await client.GetTokenAsync("helloapi");

            // Assert
            token.Should().NotBeNull();
            token.Code.Should().Be(0);
        }

        [Fact]
        public async Task GetTaskAsync_Should_Return_Task()
        {
            // Arrange
            var client = AiDevsExercisesClientFactory.Create();
            var token = await client.GetTokenAsync("helloapi");

            // Act
            var task = await client.GetTaskAsync(token.Token);

            // Assert
            task.Should().NotBeNull();
            task.Code.Should().Be(0);
        }

        [Fact]
        public async Task SendResponseAsyncc_Should_Send_Correct_Answer()
        {
            // Arrange
            var client = AiDevsExercisesClientFactory.Create();
            var token = await client.GetTokenAsync("helloapi");
            var task = await client.GetTaskAsync(token.Token);

            _testOutputHelper.WriteLine("Task message: {0}", task.Msg);

            // Act
            var answer = task.AdditionalData["cookie"].ToString();
            var result = await client.SendResponseAsync(token.Token, answer);

            // Assert
            result.Should().NotBeNull();
            result.Code.Should().Be(0);

            _testOutputHelper.WriteLine("Result message: {0}", result.Msg);
        }
    }
}
