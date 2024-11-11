using AutoTraderApp.Domain.Common;
using AutoTraderApp.Domain.Enums;

namespace AutoTraderApp.Domain.Entities;

public class Signal : BaseEntity
{
    public Guid StrategyId { get; set; }
    public Strategy Strategy { get; set; } = null!;
    public Guid InstrumentId { get; set; }
    public Instrument Instrument { get; set; } = null!;
    public SignalType Type { get; set; }
    public decimal Price { get; set; }
    public decimal Confidence { get; set; }
    public SignalStatus Status { get; set; }
}