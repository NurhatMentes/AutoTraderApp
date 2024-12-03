namespace AutoTraderApp.Domain.ExternalModels.Alpaca.Models;

public class LastPrice
{
    public decimal Price { get; set; }
    public string Symbol { get; set; }
    public DateTime Timestamp { get; set; }
}