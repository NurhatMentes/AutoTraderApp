using AutoMapper;
using AutoTraderApp.Application.Features.UserTradingAccounts.Commands.CreateUserTradingAccount;
using AutoTraderApp.Application.Features.UserTradingAccounts.DTOs;
using AutoTraderApp.Domain.Entities;

namespace AutoTraderApp.Application.Features.UserTradingAccounts.Profiles
{
    public class UserTradingAccountProfile : Profile
    {
        public UserTradingAccountProfile()
        {
            CreateMap<UserTradingAccount, UserTradingAccountDto>();

            CreateMap<CreateUserTradingAccountCommand, UserTradingAccount>()
                .ForMember(dest => dest.EncryptedPassword, opt => opt.Ignore())
                .ForMember(dest => dest.PasswordSalt, opt => opt.Ignore());
        }
    }
}
