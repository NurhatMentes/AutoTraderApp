using AutoTraderApp.Domain.Common;

namespace AutoTraderApp.Domain.Entities;

public class TradingViewLog : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid StrategyId { get; set; }
    public Guid BrokerAccountId { get; set; }
    public string Step { get; set; }  
    public string Status { get; set; } 
    public string Symbol { get; set; } 
    public string Message { get; set; }

    public User User { get; set; }
    public BrokerAccount BrokerAccount { get; set; }
    public Strategy Strategy { get; set; }
}