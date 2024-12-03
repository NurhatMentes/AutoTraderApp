using AutoTraderApp.Domain.Common;

namespace AutoTraderApp.Domain.Entities;

public class Trade : BaseEntity
{
    public Guid BrokerAccountId { get; set; }
    public BrokerAccount BrokerAccount { get; set; } = null!;
    public string Symbol { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
    public DateTime Timestamp { get; set; }
}