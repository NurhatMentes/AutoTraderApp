using AutoTraderApp.Domain.Common;

namespace AutoTraderApp.Domain.Entities;

public class Strategy : BaseEntity
{
    public string StrategyName { get; set; }
    public string Symbol { get; set; }
    public decimal EntryPrice { get; set; }
    public decimal StopLoss { get; set; }
    public decimal TakeProfit { get; set; }
    public string TimeFrame { get; set; }
    public bool IsActive { get; set; } = true;
    public string WebhookUrl { get; set; }
}