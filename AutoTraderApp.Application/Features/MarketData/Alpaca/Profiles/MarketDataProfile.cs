using AutoMapper;
using AutoTraderApp.Application.Features.MarketData.Alpaca.DTOs;
using AutoTraderApp.Domain.ExternalModels.Alpaca.Models;

namespace AutoTraderApp.Application.Features.MarketData.Alpaca.Profiles
{
    public class MarketDataProfile : Profile
    {
        public MarketDataProfile()
        {
            CreateMap<MarketDataResponse, MarketDataDto>()
                .ForMember(dest => dest.Symbol, opt => opt.MapFrom(src => src.Symbol))
                .ForMember(dest => dest.LastPrice, opt => opt.MapFrom(src => src.LastPrice))
                .ForMember(dest => dest.ChangePercent, opt => opt.MapFrom(src => src.ChangePercent))
                .ForMember(dest => dest.Volume, opt => opt.MapFrom(src => src.Volume))
                .ReverseMap();
        }
    }
}
