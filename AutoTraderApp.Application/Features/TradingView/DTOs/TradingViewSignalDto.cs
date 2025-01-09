namespace AutoTraderApp.Application.Features.TradingView.DTOs;

public class TradingViewSignalDto
{
    public Guid BrokerAccountId { get; set; }
    public Guid UserId { get; set; }
    public string Action { get; set; } 
    public string Symbol { get; set; } 
    public int Quantity { get; set; }  
}