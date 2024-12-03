using AutoTraderApp.Domain.ExternalModels.Alpaca.Models;
using System.Text.Json.Serialization;

namespace AutoTraderApp.Application.Features.Orders.DTOs;

public class PlaceOrderDto
{
    public Guid BrokerAccountId { get; set; }

    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } // Örn: "AAPL"

    [JsonPropertyName("qty")]
    public decimal Qty { get; set; } // Pozitif tam sayı (string olarak)

    [JsonPropertyName("side")]
    public string Side { get; set; } // "buy" veya "sell"

    [JsonPropertyName("type")]
    public string Type { get; set; } // Örn: "market", "limit", "stop"

    [JsonPropertyName("time_in_force")]
    public string TimeInForce { get; set; } // "gtc", "day", vb.

    [JsonPropertyName("limit_price")]
    public decimal? LimitPrice { get; set; } // Limit emri fiyatı

    [JsonPropertyName("stop_price")]
    public decimal? StopPrice { get; set; } // Stop emri fiyatı

    [JsonPropertyName("take_profit")]
    public TakeProfit? TakeProfit { get; set; } // Kar al emri için alt obje

    [JsonPropertyName("stop_loss")]
    public StopLoss? StopLoss { get; set; } // Zarar durdur emri için alt obje
}