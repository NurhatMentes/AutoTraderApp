using System.Text.Json.Serialization;

namespace AutoTraderApp.Domain.ExternalModels.Alpaca.Models;

public class StopLoss
{
    [JsonPropertyName("stop_price")]
    public decimal StopPrice { get; set; }

    [JsonPropertyName("limit_price")]
    public decimal? LimitPrice { get; set; }
}