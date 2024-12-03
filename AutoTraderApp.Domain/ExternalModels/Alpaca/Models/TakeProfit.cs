using System.Text.Json.Serialization;

namespace AutoTraderApp.Domain.ExternalModels.Alpaca.Models;

public class TakeProfit
{
    [JsonPropertyName("limit_price")]
    public decimal LimitPrice { get; set; }
}
