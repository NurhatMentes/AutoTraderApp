using AutoTraderApp.Application.Features.CombinedStocks.DTOs;
using AutoTraderApp.Domain.Entities;
using AutoMapper;

namespace AutoTraderApp.Application.Features.CombinedStocks.Profiles
{
    public class CombinedStockProfile : Profile
    {
        public CombinedStockProfile()
        {
            CreateMap<CombinedStock, CombinedStockDto>().ForMember(dest => dest.LastUpdated, opt => opt.MapFrom(src => src.UpdatedAt)).ReverseMap();
        }
    }
}
