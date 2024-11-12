using AutoMapper;
using AutoTraderApp.Application.Features.Orders.DTOs;
using AutoTraderApp.Domain.Entities;

namespace AutoTraderApp.Application.Features.Orders.Profiles
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<Order, OrderDto>()
                .ForMember(dest => dest.InstrumentSymbol, opt =>
                    opt.MapFrom(src => src.Instrument.Symbol))
                .ForMember(dest => dest.BrokerAccountName, opt =>
                    opt.MapFrom(src => src.BrokerAccount.Name)).ReverseMap();
        }
    }
}
