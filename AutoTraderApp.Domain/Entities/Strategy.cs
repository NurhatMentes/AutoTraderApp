using AutoTraderApp.Domain.Common;
using AutoTraderApp.Domain.Enums;

namespace AutoTraderApp.Domain.Entities;

public class Strategy : BaseEntity
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public StrategyStatus Status { get; set; }
    public RiskLevel RiskLevel { get; set; }
    public decimal MaxPositionSize { get; set; }
    public decimal StopLossPercentage { get; set; }
    public decimal TakeProfitPercentage { get; set; }

    public ICollection<TradingRule> TradingRules { get; set; } = new List<TradingRule>();
}