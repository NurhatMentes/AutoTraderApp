using AutoTraderApp.Application.Features.Instruments.DTOs;
using AutoMapper;
using AutoTraderApp.Domain.Entities;

namespace AutoTraderApp.Application.Features.Instruments.Profile
{
    public class InstrumentProfile : AutoMapper.Profile
    {
        public InstrumentProfile()
        {
            CreateMap<Instrument, InstrumentDto>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
        }
    }
}
