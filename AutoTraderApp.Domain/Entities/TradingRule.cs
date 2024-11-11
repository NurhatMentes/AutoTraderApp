using AutoTraderApp.Domain.Common;
using AutoTraderApp.Domain.Enums;

namespace AutoTraderApp.Domain.Entities;

public class TradingRule : BaseEntity
{
    public Guid StrategyId { get; set; }
    public Strategy Strategy { get; set; } = null!;
    public string Indicator { get; set; }
    public string Condition { get; set; }
    public decimal Value { get; set; }
    public TradingAction Action { get; set; }
}