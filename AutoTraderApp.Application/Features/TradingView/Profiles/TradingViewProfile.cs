using AutoMapper;
using AutoTraderApp.Application.Features.TradingView.DTOs;
using AutoTraderApp.Domain.ExternalModels.Alpaca.Models;
using AutoTraderApp.Domain.ExternalModels.TradingView;

namespace AutoTraderApp.Application.Features.TradingView.Profiles
{
    public class TradingViewProfile : Profile
    {
        public TradingViewProfile()
        {
            CreateMap<TradingViewStrategyDto, TradingViewStrategy>().ReverseMap();
            CreateMap<TradingViewSignalDto, OrderRequest>()
                .ForMember(dest => dest.Symbol, opt => opt.MapFrom(src => src.Symbol))
                .ForMember(dest => dest.Qty, opt => opt.MapFrom(src => src.Quantity))
                .ForMember(dest => dest.Side, opt => opt.MapFrom(src => src.Action.ToLower()))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => "market"))
                .ForMember(dest => dest.TimeInForce, opt => opt.MapFrom(src => "gtc"));
        }
    }
}
