namespace AutoTraderApp.Domain.ExternalModels.Alpaca.Models;

public class DailyTradeDetails
{
    public string Symbol { get; set; }
    public decimal TotalBuyAmount { get; set; }
    public decimal TotalSellAmount { get; set; }
    public decimal TotalBuyQuantity { get; set; }
    public decimal TotalSellQuantity { get; set; }
    public decimal PnL { get; set; }
    public decimal PnLPercentage { get; set; }
    public bool StopLossSale { get; set; }
}

