using System.Text.Json;

namespace AIDevs.Shared.Abstraction.Api.Answer
{
    public record AnswerResponse(int Code, string Msg, string Note)
    {
        [System.Text.Json.Serialization.JsonExtensionData]
        public IDictionary<string, JsonElement> AdditionalData { get; set; }
    }
}
