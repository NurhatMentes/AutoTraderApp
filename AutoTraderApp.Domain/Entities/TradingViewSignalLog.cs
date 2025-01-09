using AutoTraderApp.Domain.Common;

namespace AutoTraderApp.Domain.Entities
{
    public class TradingViewSignalLog : BaseEntity
    {
        public Guid BrokerAccountId { get; set; }
        public Guid UserId { get; set; }
        public string Action { get; set; } 
        public string Symbol { get; set; } 
        public decimal Quantity { get; set; }
        public decimal Price { get; set; } 
        public string Status { get; set; }
        public string? Message { get; set; }

        public User User { get; set; }
        public BrokerAccount BrokerAccount { get; set; }
    }
}
