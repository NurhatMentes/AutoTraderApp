using AutoTraderApp.Domain.Common;
using AutoTraderApp.Domain.Enums;

namespace AutoTraderApp.Domain.Entities;

public class Position : BaseEntity
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid BrokerAccountId { get; set; }
    public BrokerAccount BrokerAccount { get; set; } = null!;

    public Guid InstrumentId { get; set; }
    public Instrument Instrument { get; set; } = null!;

    public Guid? StrategyId { get; set; }
    public Strategy? Strategy { get; set; }

    public decimal Quantity { get; set; }
    public decimal EntryPrice { get; set; }
    public decimal CurrentPrice { get; set; }
    public decimal UnrealizedPnL { get; set; }
    public decimal RealizedPnL { get; set; }
    public PositionSide Side { get; set; }
    public PositionStatus Status { get; set; }

    public decimal? StopLoss { get; set; }
    public decimal? TakeProfit { get; set; }

    public DateTime OpenedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }

    public ICollection<Order> Orders { get; set; } = new List<Order>();
}