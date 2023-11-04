using AIDevs.Shared.Infrastructure.FunctionCalling;
using AIDevs.Shared.Infrastructure.FunctionCalling.Examples;
using AIDevs.Shared.Infrastructure.FunctionCalling.Extensions;
using Azure.AI.OpenAI;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AIDevs.Tests.Unit.Experiments
{
    public class GetDateTimeFunctionCallTests
    {

        private const string CHAT_COMPLETIONS_MODEL_NAME = "gpt-3.5-turbo";
        private readonly ITestOutputHelper _testOutputHelper;
        private OpenAIClient _azureOpenAiClient;
        private ServiceProvider _serviceProvider;
        private IFunctionCallDispatcher _functionCallDispatcher;

        public GetDateTimeFunctionCallTests(ITestOutputHelper testOutputHelper)
        {
            _azureOpenAiClient = AzureOpenAiClientFactory.Create();

            _serviceProvider = new ServiceCollection()
                .AddFunctionCalling()
                .BuildServiceProvider();

            _functionCallDispatcher = _serviceProvider.GetRequiredService<IFunctionCallDispatcher>();
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task Should_Call_Function()
        {
            // Arrange
            var completionsOptions = new ChatCompletionsOptions();

            // Defining available functions
            completionsOptions.Functions.Add(FunctionDefinitionExtensions.Create<GetDateTime>());

            // Adding a prompt using a defined function
            completionsOptions.Messages.Add(new(ChatRole.User, "Hello, what time is it?"));


            // Act
            var completionResult = await _azureOpenAiClient.GetChatCompletionsAsync(CHAT_COMPLETIONS_MODEL_NAME, completionsOptions);

            var message = completionResult.Value.Choices.First().Message;

            // Checking if the message contains a function call
            if (message?.FunctionCall?.Name is not null)
            {
                // Function call
                var callResult = await _functionCallDispatcher.DispatchAsync(message.FunctionCall.Name, message.FunctionCall.Arguments);

                completionsOptions.Messages.Add(message);
                completionsOptions.Messages.Add(callResult.ToChatMessage(message.FunctionCall.Name));

                completionResult = await _azureOpenAiClient.GetChatCompletionsAsync(CHAT_COMPLETIONS_MODEL_NAME, completionsOptions);
            }

            _testOutputHelper.WriteLine(completionResult.Value.Choices[0].Message.Content);

        }
    }
}
