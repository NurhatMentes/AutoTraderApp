using AutoMapper;
using AutoTraderApp.Application.Features.NasdaqStocks.DTOs;
using AutoTraderApp.Domain.Entities;

namespace AutoTraderApp.Application.Features.NasdaqStocks.Profiles
{
    public class NasdaqStockProfile : Profile
    {
        public NasdaqStockProfile() {
            CreateMap<NasdaqStock, NasdaqStockDto>().ReverseMap();
        }
    }
}
