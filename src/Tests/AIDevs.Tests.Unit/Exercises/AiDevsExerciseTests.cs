using AIDevs.Shared.Infrastructure.FunctionCalling;
using AIDevs.Shared.Infrastructure.FunctionCalling.Extensions;
using AIDevs.Shared.Infrastructure.OpenAi;
using AIDevs.Tests.Unit.Exercises.FunctionCalling;
using Azure.AI.OpenAI;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace AIDevs.Tests.Unit.Exercises
{
    public class AiDevsExerciseTests : AiDevsExerciseBaseTests
    {
        private const string CHAT_COMPLETIONS_MODEL_NAME = "gpt-3.5-turbo-1106";
        private const string EMBEDDING_MODEL_NAME = "text-embedding-ada-002";
        private const string AUDIO_MODEL_NAME = "whisper-1";
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

            foreach (var paragraph in paragraphs)
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

                var completionOptions = new ChatCompletionsOptions();

                completionOptions.Messages.Add(new(
                    ChatRole.System,
                    "You are the author of a blog. The blog is written in Polish. You prepare your blog post divided into chapters."));

                completionOptions.Messages.Add(new(
                    ChatRole.User,
                    $"Prepare a chapter of your blog post entitled: '{i} - {chapter}'."));

                var azureOpenAiCompletionResult = await AzureOpenAiClient.GetChatCompletionsAsync(CHAT_COMPLETIONS_MODEL_NAME, completionOptions);

                var text = azureOpenAiCompletionResult.Value.Choices.Select(x => x.Message.Content).FirstOrDefault();
                answer.Add(text);
            }

            var result = await ExercisesClient.SendResponseAsync(token.Token, answer);

            // Assert
            TestOutputHelper.WriteLine("Result message: {0}", result.Msg);

            result.Should().NotBeNull();
            result.Code.Should().Be(0);

        }

        [Theory(DisplayName = "Exercise 04 - liar")]
        [InlineData("What is capital of Poland?")]
        [InlineData("When did World War II end?")]
        [InlineData("Who was the first man to step on the moon?")]
        public async Task Should_Send_Answer_For_Liar_Exercise(string question)
        {
            // Arrange
            var token = await ExercisesClient.GetTokenAsync("liar");
            var task = await ExercisesClient.GetTaskAsync(token.Token);

            // Act

            var questionResult = await ExercisesClient.PostTaskAsync(token.Token, new Dictionary<string, string>
            {
                { "question",question }
            });

            var questionAnswer = questionResult.AdditionalData["answer"].ToString();
            TestOutputHelper.WriteLine("Question: {0}", question);
            TestOutputHelper.WriteLine("Answer: {0}", questionAnswer);

            var completionOptions = new ChatCompletionsOptions();

            completionOptions.Temperature = 0;

            completionOptions.Messages.Add(new(
                ChatRole.System,
                "You are a filtration system called Guardialis. " +
                "Your task is check answer for asked question. " +
                "If answer is related with question you have to return 'YES'. " +
                "In other wise you have to return 'NO'. " +
                "As a system you can only use 'YES' or 'NO' answer."));

            completionOptions.Messages.Add(new(
                ChatRole.User,
                $"Does the response '{questionAnswer}' answer the question '{question}'?"));

            var completionResult = await AzureOpenAiClient.GetChatCompletionsAsync(CHAT_COMPLETIONS_MODEL_NAME, completionOptions);

            var completionResultMessage = completionResult.Value.Choices.Select(x => x.Message.Content).FirstOrDefault();
            TestOutputHelper.WriteLine("Is the answer related to the question?: {0}", completionResultMessage);

            var result = await ExercisesClient.SendResponseAsync(token.Token, completionResultMessage);

            // Assert
            TestOutputHelper.WriteLine("Result message: {0}", result.Msg);

            result.Should().NotBeNull();
            result.Code.Should().Be(0);
        }

        [Fact(DisplayName = "Exercise 05 - inprompt")]
        public async Task Shoud_Filter_Input_And_Answer_The_Question()
        {
            // Arrange
            var token = await ExercisesClient.GetTokenAsync("inprompt");
            var task = await ExercisesClient.GetTaskAsync(token.Token);

            var input = task.AdditionalData["input"].Deserialize<string[]>();
            var question = task.AdditionalData["question"].Deserialize<string>();

            // Act

            var name = await FindName(question);
            var filter = input.Where(text => text.Contains(name, StringComparison.InvariantCultureIgnoreCase));

            var answer = await AnswerQuestion(question, filter);

            TestOutputHelper.WriteLine("Question: {0}", question);
            TestOutputHelper.WriteLine("Answer: {0}", answer);


            var result = await ExercisesClient.SendResponseAsync(token.Token, answer);

            // Assert
            TestOutputHelper.WriteLine("Result message: {0}", result.Msg);

            result.Should().NotBeNull();
            result.Code.Should().Be(0);

            async Task<string> FindName(string question)
            {
                var completionOptions = new ChatCompletionsOptions();

                completionOptions.Temperature = 0;

                completionOptions.Messages.Add(new(
                    role: ChatRole.System,
                    content: "Recognise and return the names contained in the text"
                    ));

                completionOptions.Messages.Add(new(
                    role: ChatRole.User,
                    content: "How old is Adam?"
                    ));

                completionOptions.Messages.Add(new(
                    role: ChatRole.Assistant,
                    content: "Adam"));

                completionOptions.Messages.Add(new(
                    role: ChatRole.User,
                    content: question));

                var completionResult = await AzureOpenAiClient.GetChatCompletionsAsync(CHAT_COMPLETIONS_MODEL_NAME, completionOptions);

                var completionResultMessage = completionResult.Value.Choices.Select(x => x.Message.Content).FirstOrDefault();

                return completionResultMessage;
            }

            async Task<string> AnswerQuestion(string question, IEnumerable<string> input)
            {
                var completionOptions = new ChatCompletionsOptions();

                completionOptions.Temperature = 0;

                completionOptions.Messages.Add(new(
                    role: ChatRole.System,
                    content: $@"Answer the question based on the facts: {string.Join(" ", input)}"
                    ));


                completionOptions.Messages.Add(new(
                    role: ChatRole.User,
                    content: question));

                var completionResult = await AzureOpenAiClient.GetChatCompletionsAsync(CHAT_COMPLETIONS_MODEL_NAME, completionOptions);

                var completionResultMessage = completionResult.Value.Choices.Select(x => x.Message.Content).FirstOrDefault();

                return completionResultMessage;
            }
        }

        [Fact(DisplayName = "Exercise 06 - embedding")]
        public async Task Should_Generate_Embedding()
        {
            // Arrange
            var token = await ExercisesClient.GetTokenAsync("embedding");
            var task = await ExercisesClient.GetTaskAsync(token.Token);

            // Act
            var input = "Hawaiian pizza";
            var embeddingResult = await AzureOpenAiClient.GetEmbeddingsAsync(EMBEDDING_MODEL_NAME, new EmbeddingsOptions(input));
            var embedding = embeddingResult.Value.Data[0].Embedding.ToArray();

            var result = await ExercisesClient.SendResponseAsync(token.Token, embedding);

            // Assert
            TestOutputHelper.WriteLine("Result message: {0}", result.Msg);

            result.Should().NotBeNull();
            result.Code.Should().Be(0);
        }

        [Fact(DisplayName = "Exercise 06 - whisper")]
        public async Task Should_Generate_Transcription()
        {
            // Arrange
            var token = await ExercisesClient.GetTokenAsync("whisper");
            var task = await ExercisesClient.GetTaskAsync(token.Token);

            // Act
            using var httpCLient = new HttpClient();
            var bytes = await httpCLient.GetByteArrayAsync("https://zadania.aidevs.pl/data/mateusz.mp3");

            var transctiptionResult = await AzureOpenAiClient.GetAudioTranscriptionAsync(AUDIO_MODEL_NAME, new AudioTranscriptionOptions(BinaryData.FromBytes(bytes)));
            var transcription = transctiptionResult.Value.Text;

            var result = await ExercisesClient.SendResponseAsync(token.Token, transcription);

            // Assert
            TestOutputHelper.WriteLine("Result message: {0}", result.Msg);

            result.Should().NotBeNull();
            result.Code.Should().Be(0);
        }

        [Fact(DisplayName = "Exercise 08 - rodo")]
        public async Task Should_Use_Placeholders()
        {
            // Arrange
            var token = await ExercisesClient.GetTokenAsync("rodo");
            var task = await ExercisesClient.GetTaskAsync(token.Token);

            // Act
            var answer =
                @"Tell me something about your self. 
                Instate of sensitive information use placeholders like: %imie%, %nazwisko%, %miasto% i %zawod%";

            var result = await ExercisesClient.SendResponseAsync(token.Token, answer);

            // Assert
            TestOutputHelper.WriteLine("Result message: {0}", result.Msg);

            result.Should().NotBeNull();
            result.Code.Should().Be(0);

        }

        [Fact(DisplayName = "Exercise 09 - scraper")]
        public async Task Should_Handle_Errors()
        {
            // Arrange
            var serviceProvider = new ServiceCollection()
                .AddFunctionCalling()
                .BuildServiceProvider();

            var functionDispatcher = serviceProvider.GetRequiredService<IFunctionCallDispatcher>();

            var token = await ExercisesClient.GetTokenAsync("scraper");
            var task = await ExercisesClient.GetTaskAsync(token.Token);

            var input = task.AdditionalData["input"].ToString();
            var question = task.AdditionalData["question"].ToString();

            var completionsOptions = new ChatCompletionsOptions();

            completionsOptions.Functions.Add(FunctionDefinitionExtensions.Create<ScraperExerciseFunctionCall>());

            completionsOptions.Messages.Add(new(ChatRole.System, 
                $@"Answer the question. Rules of answer: 
    1. Information about questions is in service URL: '{input}'
    2. Answer only in Polish language
    3  Answer must have maximum 20 words."));
            completionsOptions.Messages.Add(new(ChatRole.User, question));

            // Act
            var completionResult = await AzureOpenAiClient.GetChatCompletionsAsync(CHAT_COMPLETIONS_MODEL_NAME, completionsOptions);

            var message = completionResult.Value.Choices.First().Message;

            message.FunctionCall.Should().NotBeNull();

            // Checking if the message contains a function call
            if (message?.FunctionCall?.Name is not null)
            {
                // Function call
                TestOutputHelper.WriteLine($"Assistant: Calling the '{message.FunctionCall.Name}' function...");

                var callResult = await functionDispatcher.DispatchAsync(message.FunctionCall.Name, message.FunctionCall.Arguments);

                completionsOptions.Messages.Add(message);

                var functionMessage = callResult.ToChatMessage(message.FunctionCall.Name);

                TestOutputHelper.WriteLine($"Function: {functionMessage.Content}");

                completionsOptions.Messages.Add(functionMessage);

                completionResult = await AzureOpenAiClient.GetChatCompletionsAsync(CHAT_COMPLETIONS_MODEL_NAME, completionsOptions);
            }

            var answer = completionResult.Value.Choices[0].Message.Content;
            TestOutputHelper.WriteLine($"Assistant: {answer}");


            var result = await ExercisesClient.SendResponseAsync(token.Token, answer);

            // Assert
            TestOutputHelper.WriteLine("Result message: {0}", result.Msg);

            result.Should().NotBeNull();
            result.Code.Should().Be(0);
        }
    }
}
