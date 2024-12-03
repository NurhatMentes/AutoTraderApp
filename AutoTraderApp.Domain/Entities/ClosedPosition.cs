using AutoTraderApp.Domain.Common;

namespace AutoTraderApp.Domain.Entities;

public class ClosedPosition : BaseEntity
{
    public string Symbol { get; set; }
    public decimal Quantity { get; set; }
    public decimal RealizedPnL { get; set; }
    public DateTime ClosedAt { get; set; }
}