using AutoMapper;
using AutoTraderApp.Application.Features.Prices.DTOs;
using AutoTraderApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoTraderApp.Application.Features.Prices.Profiles
{
    public class PriceProfile : Profile
    {
        public PriceProfile()
        {
            CreateMap<Price, PriceDto>().ReverseMap();
        }
    }
}
