using AIDevs.Shared.Infrastructure.Splitters;
using Azure.AI.OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIDevs.Tests.Unit.Experiments
{
    public class TextSummarizeTests
    {
        private const string CHAT_COMPLETIONS_MODEL_NAME = "gpt-3.5-turbo-1106";
        //private const string CHAT_COMPLETIONS_MODEL_NAME = "gpt-4";

        private OpenAIClient _azureOpenAiClient;

        public TextSummarizeTests()
        {
            _azureOpenAiClient = AzureOpenAiClientFactory.Create();
        }

        [Fact]
        public async Task Shoudl_Summarize_Text()
        {
            // Arrange
            var input = File.ReadAllText("Assets/LongText.txt");

            var splitter = new TextSplitter(separator: ".", chunkSize: 6000);
            var chunks = splitter.SplitText(input);

            var systemPrompt =
@"You are a cutting-edge AI summarization system designed to generate concise summaries based on input (document, podcast transcription, interview, presentation). 
Your goal is to accurately distill the key points from a document while ensuring that the essential information is preserved. 
Please use the fragment provided by the user, along with the previous summary of the document, to generate a cohesive and informative summary. 
Make sure to maintain the coherence and context established by the preceding summary. 
Feel free to consult the previous summary for reference, and provide a summary that encapsulates the main ideas and crucial details from the given text fragment.
Write only in Polish, even if the document is written entirely in English.";

            var result = new List<string>();

            // Act

            foreach(var chunk in chunks) 
            {
                var summary = await Summarize(chunk, result.LastOrDefault());
                result.Add(summary);
            }

            var join = string.Join($"{Environment.NewLine}{Environment.NewLine}", result);
            File.WriteAllText("summary.txt", join);

            var tldr = await Summarize(join, null);
            File.WriteAllText("tldr.txt", tldr);

            // Assert
            result.Should().HaveCount(chunks.Count);

            async Task<string> Summarize(string chunk, string? previousPart)
            {
                var completionsOptions = new ChatCompletionsOptions();

                completionsOptions.Temperature = 1.0f;
                completionsOptions.MaxTokens = 400;

                completionsOptions.Messages.Add(new(ChatRole.System, systemPrompt));

                if (previousPart is not null)
                {
                    completionsOptions.Messages.Add(new(ChatRole.System,
$@"Previous document part summary:

{previousPart}"));
                }

                completionsOptions.Messages.Add(new(ChatRole.User, $"###{chunk}###"));

                var completionResult = await _azureOpenAiClient.GetChatCompletionsAsync(CHAT_COMPLETIONS_MODEL_NAME, completionsOptions);

                return completionResult.Value.Choices.First().Message.Content;
            }
        }
    }
}
