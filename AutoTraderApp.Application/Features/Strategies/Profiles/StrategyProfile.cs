using AutoMapper;
using AutoTraderApp.Application.Features.Strategies.Commands;
using AutoTraderApp.Application.Features.Strategies.DTOs;
using AutoTraderApp.Application.Features.TradingView.DTOs;
using AutoTraderApp.Domain.Entities;

namespace AutoTraderApp.Application.Features.Strategies.Profiles
{
    public class StrategyProfile : Profile
    {
        public StrategyProfile()
        {
            CreateMap<GenerateStrategyCommand, StrategyDto>().ReverseMap();
            CreateMap<TradingViewStrategyDto, Strategy>().ReverseMap();
            CreateMap<Strategy, StrategyDto>().ForMember(dest => dest.WebhookUrl, opt => opt.MapFrom(src => src.WebhookUrl)).ReverseMap();
        }
    }
}
