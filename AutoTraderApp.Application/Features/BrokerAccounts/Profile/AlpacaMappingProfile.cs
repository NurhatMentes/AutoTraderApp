using AutoTraderApp.Application.Features.BrokerAccounts.DTOs;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Domain.ExternalModels.Alpaca.Models;

namespace AutoTraderApp.Application.Features.BrokerAccounts.Profile
{
    public class BrokerAccountMappingProfile : AutoMapper.Profile
    {
        public BrokerAccountMappingProfile()
        {
            CreateMap<BrokerAccount, BrokerAccountDto>().ReverseMap();
            CreateMap<AddBrokerAccountDto, BrokerAccount>().ReverseMap();
            CreateMap<AccountInfo, AccountInfoDto>()
                .ForMember(dest => dest.AccountId, opt => opt.MapFrom(src => src.AccountId))
                .ForMember(dest => dest.Cash, opt => opt.MapFrom(src => src.Cash))
                .ForMember(dest => dest.BuyingPower, opt => opt.MapFrom(src => src.BuyingPower))
                .ForMember(dest => dest.PortfolioValue, opt => opt.MapFrom(src => src.PortfolioValue))
                .ForMember(dest => dest.IsPaper, opt => opt.MapFrom(src => src.IsPaper))
                .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.Currency))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

        }
    }

}
