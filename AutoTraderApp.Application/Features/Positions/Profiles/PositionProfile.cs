using AutoMapper;
using AutoTraderApp.Application.Features.Positions.DTOs;
using AutoTraderApp.Domain.Entities;

namespace AutoTraderApp.Application.Features.Positions.Profiles
{
    public class PositionProfile : Profile
    {
        public PositionProfile()
        {
            CreateMap<Position, PositionDto>()
                .ForMember(dest => dest.InstrumentSymbol, opt =>
                    opt.MapFrom(src => src.Instrument.Symbol))
                .ForMember(dest => dest.BrokerAccountName, opt =>
                    opt.MapFrom(src => src.BrokerAccount.Name)).ReverseMap();
        }
    }

}
