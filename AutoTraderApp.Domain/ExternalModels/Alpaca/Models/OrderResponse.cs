using System.Text.Json.Serialization;

namespace AutoTraderApp.Domain.ExternalModels.Alpaca.Models;

public class OrderResponse
{
    [JsonPropertyName("id")]
    public string OrderId { get; set; }

    [JsonPropertyName("symbol")]
    public string Symbol { get; set; }

    [JsonPropertyName("qty")]
    public string Quantity { get; set; }

    [JsonPropertyName("side")]
    public string Side { get; set; }

    [JsonPropertyName("type")]
    public string Type { get; set; }

    [JsonPropertyName("time_in_force")]
    public string TimeInForce { get; set; }

    [JsonPropertyName("limit_price")]
    public string? LimitPrice { get; set; }

    [JsonPropertyName("stop_price")]
    public string? StopPrice { get; set; }

    [JsonPropertyName("order_class")]
    public string? OrderClass { get; set; }

    [JsonPropertyName("take_profit")]
    public TakeProfit? TakeProfit { get; set; }

    [JsonPropertyName("stop_loss")]
    public StopLoss? StopLoss { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; }

    [JsonPropertyName("filled_qty")]
    public string FilledQuantity { get; set; }

    [JsonPropertyName("filled_avg_price")]
    public string? FilledAvgPrice { get; set; }

    [JsonPropertyName("error_message")]
    public string? ErrorMessage { get; set; }

    [JsonPropertyName("filled_at")]
    public DateTime? FilledAt { get; set; }

    [JsonPropertyName("triggered_by")]
    public string? TriggeredBy { get; set; }

}