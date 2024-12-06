namespace AutoTraderApp.Application.Features.TradingView.DTOs;

public class TradingViewSignalDto
{
    public string Action { get; set; } 
    public string Symbol { get; set; } 
    public int Quantity { get; set; }  
    public decimal Price { get; set; }
}