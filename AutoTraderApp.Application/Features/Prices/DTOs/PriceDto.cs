using AutoMapper;
using AutoTraderApp.Application.Features.Common.Mappings;
using AutoTraderApp.Domain.Entities;

namespace AutoTraderApp.Application.Features.Prices.DTOs
{
    public class PriceDto : IMapFrom<Price>
    {
        public Guid Id { get; set; }
        public Guid InstrumentId { get; set; }
        public DateTime Timestamp { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public decimal Volume { get; set; }
    }
}
