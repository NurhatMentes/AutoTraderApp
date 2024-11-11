using AutoTraderApp.Domain.Common;
using AutoTraderApp.Domain.Enums;

namespace AutoTraderApp.Domain.Entities;

public class TradingSession : BaseEntity
{
    public Guid StrategyId { get; set; }
    public Strategy Strategy { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public decimal StartBalance { get; set; }
    public decimal CurrentBalance { get; set; }
    public int TotalTrades { get; set; }
    public decimal ProfitLoss { get; set; }
    public SessionStatus Status { get; set; }
}