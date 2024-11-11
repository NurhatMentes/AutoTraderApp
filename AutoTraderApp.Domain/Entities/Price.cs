using AutoTraderApp.Domain.Common;

namespace AutoTraderApp.Domain.Entities;

public class Price : BaseEntity
{
    public Guid InstrumentId { get; set; }
    public Instrument Instrument { get; set; } = null!;
    public DateTime Timestamp { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public decimal Volume { get; set; }
}
