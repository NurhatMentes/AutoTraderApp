using AutoTraderApp.Domain.Common;

namespace AutoTraderApp.Domain.Entities;

public class Trade : BaseEntity
{
    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;
    public Guid? PositionId { get; set; }
    public Position? Position { get; set; }
    public decimal ExecutedPrice { get; set; }
    public decimal ExecutedQuantity { get; set; }
    public decimal Commission { get; set; }
    public string? ExternalTradeId { get; set; }
    public DateTime ExecutedAt { get; set; }
}