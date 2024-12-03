using System.Text.Json.Serialization;

public class PositionResponse
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; }

    [JsonPropertyName("qty")]
    public string Quantity { get; set; }

    [JsonPropertyName("avg_entry_price")]
    public string EntryPrice { get; set; }

    [JsonPropertyName("market_value")]
    public string MarketValue { get; set; }

    [JsonPropertyName("cost_basis")]
    public string CostBasis { get; set; }

    [JsonPropertyName("unrealized_pl")]
    public string UnrealizedPnL { get; set; }

    [JsonPropertyName("unrealized_plpc")]
    public string UnrealizedPnLPercentage { get; set; }

    [JsonPropertyName("current_price")]
    public string CurrentPrice { get; set; }

    [JsonPropertyName("realized_pl")]
    public string RealizedPnL { get; set; }

    [JsonPropertyName("change_today")]
    public string TodayChange { get; set; }

    [JsonPropertyName("qty_available")]
    public string AvailableQuantity { get; set; }
}
