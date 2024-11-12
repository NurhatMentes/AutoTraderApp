using AutoTraderApp.Domain.Enums;

namespace AutoTraderApp.Application.Features.Orders.DTOs
{
    public class OrderDto
    {
        public Guid Id { get; set; }
        public string InstrumentSymbol { get; set; }
        public string BrokerAccountName { get; set; }
        public OrderType Type { get; set; }
        public OrderSide Side { get; set; }
        public decimal Quantity { get; set; }
        public decimal? Price { get; set; }
        public decimal? StopLoss { get; set; }
        public decimal? TakeProfit { get; set; }
        public OrderStatus Status { get; set; }
        public string ExternalOrderId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

}
