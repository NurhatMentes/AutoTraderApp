using AutoTraderApp.Domain.Enums;

namespace AutoTraderApp.Application.Features.Strategies.DTOs;

public class CreateStrategyDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public RiskLevel RiskLevel { get; set; }
    public StrategyStatus Status { get; set; }
}