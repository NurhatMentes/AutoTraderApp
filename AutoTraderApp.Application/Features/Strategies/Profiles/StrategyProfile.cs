using AutoMapper;
using AutoTraderApp.Application.Features.Strategies.Commands;
using AutoTraderApp.Application.Features.Strategies.DTOs;

namespace AutoTraderApp.Application.Features.Strategies.Profiles
{
    public class StrategyProfile : Profile
    {
        public StrategyProfile()
        {
            CreateMap<GenerateStrategyCommand, StrategyDto>();
        }
    }
}
