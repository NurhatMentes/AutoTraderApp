using AutoTraderApp.Domain.Common;

namespace AutoTraderApp.Domain.Entities
{
    public class Order : BaseEntity
    {
        public Guid BrokerAccountId { get; set; }
        public BrokerAccount BrokerAccount { get; set; } = null!;

        public string Symbol { get; set; }
        public int Quantity { get; set; }
        public string Side { get; set; } = "buy";
        public string Type { get; set; } = "market";
        public decimal? LimitPrice { get; set; }
        public decimal? StopPrice { get; set; }
        public string TimeInForce { get; set; } = "gtc";
        public string? OrderClass { get; set; }
        public decimal? TakeProfitLimitPrice { get; set; }
        public decimal? StopLossStopPrice { get; set; }
        public decimal? StopLossLimitPrice { get; set; }
        public string Status { get; set; } = "new";
        public decimal? FilledQuantity { get; set; }
        public decimal? FilledPrice { get; set; }
    }
}