using AutoTraderApp.Domain.Enums;

namespace AutoTraderApp.Application.Features.Strategies.DTOs;

public class StrategyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public RiskLevel RiskLevel { get; set; }
    public StrategyStatus Status { get; set; }
    public decimal MaxPositionSize { get; set; }
    public decimal StopLossPercentage { get; set; }
    public decimal TakeProfitPercentage { get; set; }
    public List<TradingRuleDto> TradingRules { get; set; }
}