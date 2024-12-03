using System.Text.Json.Serialization;

namespace AutoTraderApp.Domain.ExternalModels.Alpaca.Models
{
    public class PortfolioResponse
    {
        [JsonPropertyName("symbol")]
        public string Symbol { get; set; }

        [JsonPropertyName("qty")]
        public decimal Qty { get; set; }

        [JsonPropertyName("market_value")]
        public decimal MarketValue { get; set; }

        [JsonPropertyName("cost_basis")]
        public decimal CostBasis { get; set; }

        [JsonPropertyName("unrealized_pl")]
        public decimal UnrealizedPnL { get; set; }

        [JsonPropertyName("current_price")]
        public decimal CurrentPrice { get; set; }
    }
}
