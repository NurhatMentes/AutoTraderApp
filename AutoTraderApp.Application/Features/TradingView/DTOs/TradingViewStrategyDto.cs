namespace AutoTraderApp.Application.Features.TradingView.DTOs;

public class TradingViewStrategyDto
{
    public string Name { get; set; } = null!;
    public string Symbol { get; set; } = null!;
    public decimal EntryPrice { get; set; }
    public decimal StopLoss { get; set; }
    public decimal TakeProfit { get; set; }
    public string TimeFrame { get; set; } = null!;
}