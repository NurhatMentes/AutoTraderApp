namespace AutoTraderApp.Application.Features.Position.DTOs;

public class ClosedPositionDto
{
    public string Symbol { get; set; }
    public decimal Quantity { get; set; }
    public decimal RealizedPnL { get; set; }
    public DateTime ClosedAt { get; set; }
}