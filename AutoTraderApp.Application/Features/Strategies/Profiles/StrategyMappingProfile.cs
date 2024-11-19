using AutoMapper;
using AutoTraderApp.Application.Features.Strategies.Commands.CreateStrategy;
using AutoTraderApp.Application.Features.Strategies.Commands.UpdateStrategy;
using AutoTraderApp.Application.Features.Strategies.DTOs;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Domain.Enums;

namespace AutoTraderApp.Application.Features.Strategies.Profiles
{
    public class StrategyMappingProfile : Profile
    {
        public StrategyMappingProfile()
        {
            CreateMap<Strategy, StrategyDto>()
                .ForMember(dest => dest.TradingRules, opt =>
                    opt.MapFrom(src => src.TradingRules));

            CreateMap<Strategy, StrategyListDto>()
                .ForMember(dest => dest.TradingRuleCount, opt =>
                    opt.MapFrom(src => src.TradingRules.Count));

            CreateMap<Strategy, StrategyDetailDto>()
                .ForMember(dest => dest.UserFirstName, opt =>
                    opt.MapFrom(src => src.User.FirstName))
                .ForMember(dest => dest.UserLastName, opt =>
                    opt.MapFrom(src => src.User.LastName));

            CreateMap<TradingRule, TradingRuleDto>().ReverseMap();

            CreateMap<CreateStrategyCommand, Strategy>()
                .ForMember(dest => dest.Status, opt =>
                    opt.MapFrom(src => StrategyStatus.Active))
                .ForMember(dest => dest.TradingRules, opt => opt.Ignore());

            CreateMap<UpdateStrategyCommand, Strategy>()
                .ForMember(dest => dest.TradingRules, opt => opt.Ignore());
        }
    }
}
