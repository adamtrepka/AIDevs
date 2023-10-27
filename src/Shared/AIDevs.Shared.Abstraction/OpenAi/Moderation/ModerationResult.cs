using System.Text.Json.Serialization;

namespace AIDevs.Shared.Abstraction.OpenAi.Moderation
{
    public class ModerationResult
    {
        public string id { get; set; }
        public string model { get; set; }
        public List<Result> results { get; set; }

        public class Categories
        {
            public bool sexual { get; set; }
            public bool hate { get; set; }
            public bool harassment { get; set; }

            [JsonPropertyName("self-harm")]
            public bool selfharm { get; set; }

            [JsonPropertyName("sexual/minors")]
            public bool sexualminors { get; set; }

            [JsonPropertyName("hate/threatening")]
            public bool hatethreatening { get; set; }

            [JsonPropertyName("violence/graphic")]
            public bool violencegraphic { get; set; }

            [JsonPropertyName("self-harm/intent")]
            public bool selfharmintent { get; set; }

            [JsonPropertyName("self-harm/instructions")]
            public bool selfharminstructions { get; set; }

            [JsonPropertyName("harassment/threatening")]
            public bool harassmentthreatening { get; set; }
            public bool violence { get; set; }
        }

        public class CategoryScores
        {
            public double sexual { get; set; }
            public double hate { get; set; }
            public double harassment { get; set; }

            [JsonPropertyName("self-harm")]
            public double selfharm { get; set; }

            [JsonPropertyName("sexual/minors")]
            public double sexualminors { get; set; }

            [JsonPropertyName("hate/threatening")]
            public double hatethreatening { get; set; }

            [JsonPropertyName("violence/graphic")]
            public double violencegraphic { get; set; }

            [JsonPropertyName("self-harm/intent")]
            public double selfharmintent { get; set; }

            [JsonPropertyName("self-harm/instructions")]
            public double selfharminstructions { get; set; }

            [JsonPropertyName("harassment/threatening")]
            public double harassmentthreatening { get; set; }
            public double violence { get; set; }
        }

        public class Result
        {
            public bool flagged { get; set; }
            public Categories categories { get; set; }
            public CategoryScores category_scores { get; set; }
        }
    }
}
