using AutoTraderApp.Domain.Enums;

namespace AutoTraderApp.Application.Features.Positions.DTOs
{
    public class PositionDto
    {
        public Guid Id { get; set; }
        public string InstrumentSymbol { get; set; }
        public string BrokerAccountName { get; set; }
        public decimal Quantity { get; set; }
        public decimal EntryPrice { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal UnrealizedPnL { get; set; }
        public decimal RealizedPnL { get; set; }
        public PositionSide Side { get; set; }
        public PositionStatus Status { get; set; }
        public DateTime OpenedAt { get; set; }
        public DateTime? ClosedAt { get; set; }
    }

}
