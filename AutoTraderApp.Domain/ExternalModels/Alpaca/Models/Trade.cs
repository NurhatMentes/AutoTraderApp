using System.Text.Json.Serialization;

namespace AutoTraderApp.Domain.ExternalModels.Alpaca.Models
{
    public class Trade
    {
        [JsonPropertyName("i")]
        public long ID { get; set; }

        [JsonPropertyName("t")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("x")]
        public string Exchange { get; set; }

        [JsonPropertyName("p")]
        public decimal Price { get; set; }

        [JsonPropertyName("s")]
        public decimal Size { get; set; }

        [JsonPropertyName("c")]
        public List<string> Conditions { get; set; }

        [JsonPropertyName("z")]
        public string Tape { get; set; }
    }
}
