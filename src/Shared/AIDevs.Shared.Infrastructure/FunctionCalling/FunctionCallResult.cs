using Azure.AI.OpenAI;

namespace AIDevs.Shared.Infrastructure.FunctionCalling
{
    public record FunctionCallResult(string Result)
    {
        public static FunctionCallResult Empty = new(string.Empty);

        public ChatMessage ToChatMessage(string functionCall) => new ChatMessage()
        {
            Role = ChatRole.Function,
            Name = functionCall,
            Content = Result
        };
    }
}
