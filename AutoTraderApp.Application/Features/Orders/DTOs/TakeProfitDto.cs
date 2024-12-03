using System.Text.Json.Serialization;

namespace AutoTraderApp.Application.Features.Orders.DTOs;

public class TakeProfitDto
{
    [JsonPropertyName("limit_price")]
    public string? LimitPrice { get; set; } // Kar al fiyatı
}