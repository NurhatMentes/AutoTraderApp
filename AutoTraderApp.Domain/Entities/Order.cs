using AutoTraderApp.Domain.Common;
using AutoTraderApp.Domain.Enums;

namespace AutoTraderApp.Domain.Entities;

public class Order : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public Guid? SignalId { get; set; }
    public Signal? Signal { get; set; }
    public Guid BrokerAccountId { get; set; }
    public BrokerAccount BrokerAccount { get; set; } = null!;
    public Guid InstrumentId { get; set; }
    public Instrument Instrument { get; set; } = null!;
    public OrderType Type { get; set; }
    public OrderSide Side { get; set; }
    public decimal Quantity { get; set; }
    public decimal? Price { get; set; }
    public decimal? StopLoss { get; set; }
    public decimal? TakeProfit { get; set; }
    public string? ExternalOrderId { get; set; }
    public OrderStatus Status { get; set; }
    public string? RejectionReason { get; set; }

    // Navigation property
    public ICollection<Trade> Trades { get; set; } = new List<Trade>();
}