using System.Text.Json.Serialization;

namespace AutoTraderApp.Domain.ExternalModels.Alpaca.Models;

public class OrderRequest
{
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; } 

    [JsonPropertyName("qty")]
    public int Qty { get; set; }  

    [JsonPropertyName("side")]
    public string Side { get; set; } 

    [JsonPropertyName("type")]
    public string Type { get; set; }  

    [JsonPropertyName("limit_price")]
    public decimal? LimitPrice { get; set; }  

    [JsonPropertyName("stop_price")]
    public decimal? StopPrice { get; set; }  

    [JsonPropertyName("time_in_force")]
    public string TimeInForce { get; set; } 

    [JsonPropertyName("order_class")]
    public string? OrderClass { get; set; } 

    [JsonPropertyName("take_profit")]
    public TakeProfit? TakeProfit { get; set; } 

    [JsonPropertyName("stop_loss")]
    public StopLoss? StopLoss { get; set; }     
}