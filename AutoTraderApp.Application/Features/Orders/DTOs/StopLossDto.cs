using System.Text.Json.Serialization;

namespace AutoTraderApp.Application.Features.Orders.DTOs;

public class StopLossDto
{
    [JsonPropertyName("stop_price")]
    public string? StopPrice { get; set; } // Stop fiyatı

    [JsonPropertyName("limit_price")]
    public string? LimitPrice { get; set; } // Stop limit fiyatı 
}