using AutoMapper;
using AutoTraderApp.Application.Features.Common.Mappings;
using AutoTraderApp.Domain.Enums;
using AutoTraderApp.Domain.Entities;

namespace AutoTraderApp.Application.Features.Instruments.DTOs
{
    public class InstrumentDto : IMapFrom<Instrument>
    {
        public string Symbol { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Exchange { get; set; }
        public decimal MinTradeAmount { get; set; }
        public decimal MaxTradeAmount { get; set; }
        public decimal PriceDecimalPlaces { get; set; }
        public string Status { get; set; }

    }
}
