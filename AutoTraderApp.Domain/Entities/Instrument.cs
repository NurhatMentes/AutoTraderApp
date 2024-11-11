using AutoTraderApp.Domain.Common;
using AutoTraderApp.Domain.Enums;

namespace AutoTraderApp.Domain.Entities;

public class Instrument : BaseEntity
{
    public string Symbol { get; set; }
    public string Name { get; set; }
    public InstrumentType Type { get; set; }
    public string Exchange { get; set; }
    public decimal MinTradeAmount { get; set; }
    public decimal MaxTradeAmount { get; set; }
    public decimal PriceDecimalPlaces { get; set; }
    public InstrumentStatus Status { get; set; }


    public ICollection<Price> Prices { get; set; } = new List<Price>();
}