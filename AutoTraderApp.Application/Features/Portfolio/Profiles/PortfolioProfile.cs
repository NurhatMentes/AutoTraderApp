using AutoMapper;
using AutoTraderApp.Application.Features.Portfolio.DTOs;

namespace AutoTraderApp.Application.Features.Portfolio.Profiles
{
    internal class PortfolioProfile : Profile
    {
        public PortfolioProfile()
        {
            CreateMap<Domain.Entities.Portfolio, PortfolioDto>().ReverseMap();
        }
    }
}
