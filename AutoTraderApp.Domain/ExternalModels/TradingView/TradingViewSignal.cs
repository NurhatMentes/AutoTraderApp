using System.Text.Json.Serialization;

namespace AutoTraderApp.Domain.ExternalModels.TradingView
{
    public class TradingViewStrategy
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("symbol")]
        public string Symbol { get; set; } = null!;

        [JsonPropertyName("entryPrice")]
        public decimal EntryPrice { get; set; }

        [JsonPropertyName("stopLoss")]
        public decimal StopLoss { get; set; }

        [JsonPropertyName("takeProfit")]
        public decimal TakeProfit { get; set; }

        [JsonPropertyName("timeFrame")]
        public string TimeFrame { get; set; } = null!;
    }
}
