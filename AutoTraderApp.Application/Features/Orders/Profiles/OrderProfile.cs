using AutoMapper;
using AutoTraderApp.Application.Features.Orders.DTOs;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Domain.ExternalModels.Alpaca.Models;

namespace AutoTraderApp.Application.Features.Orders.Profiles
{
    public class OrderProfile : Profile
    {
        public OrderProfile()
        {
            CreateMap<PlaceOrderDto, OrderRequest>()
                .ForMember(dest => dest.TakeProfit, opt => opt.MapFrom(src => src.TakeProfit != null ? new TakeProfit
                {
                    LimitPrice = src.TakeProfit.LimitPrice
                } : null))
                .ForMember(dest => dest.StopLoss, opt => opt.MapFrom(src => src.StopLoss != null ? new StopLoss
                {
                    StopPrice = src.StopLoss.StopPrice,
                    LimitPrice = src.StopLoss.LimitPrice
                } : null))
                .ReverseMap();

            CreateMap<OrderResponse, Order>()
                .ForMember(dest => dest.Symbol, opt => opt.MapFrom(src => src.Symbol))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => ConvertToInt(src.Quantity)))
                .ForMember(dest => dest.Side, opt => opt.MapFrom(src => src.Side))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.LimitPrice, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.LimitPrice) ? (decimal?)null : Convert.ToDecimal(src.LimitPrice)))
                .ForMember(dest => dest.StopPrice, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.StopPrice) ? (decimal?)null : Convert.ToDecimal(src.StopPrice)))
                .ForMember(dest => dest.TimeInForce, opt => opt.MapFrom(src => src.TimeInForce))
                .ForMember(dest => dest.OrderClass, opt => opt.MapFrom(src => src.OrderClass))
                .ForMember(dest => dest.TakeProfitLimitPrice, opt => opt.MapFrom(src => src.TakeProfit != null ? src.TakeProfit.LimitPrice : (decimal?)null))
                .ForMember(dest => dest.StopLossStopPrice, opt => opt.MapFrom(src => src.StopLoss != null ? src.StopLoss.StopPrice : (decimal?)null))
                .ForMember(dest => dest.StopLossLimitPrice, opt => opt.MapFrom(src => src.StopLoss != null ? src.StopLoss.LimitPrice : (decimal?)null))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.FilledQuantity, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.FilledQuantity) ? (decimal?)null : Convert.ToDecimal(src.FilledQuantity)))
                .ForMember(dest => dest.FilledPrice, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.FilledAvgPrice) ? (decimal?)null : Convert.ToDecimal(src.FilledAvgPrice)))
                .ReverseMap();

            CreateMap<OrderResponse, OrderResponseDto>()
                .ForMember(dest => dest.FilledQuantity, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.FilledQuantity) ? 0 : Convert.ToDecimal(src.FilledQuantity)))
                .ForMember(dest => dest.FilledPrice, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.FilledAvgPrice) ? (decimal?)null : Convert.ToDecimal(src.FilledAvgPrice)))
                .ForMember(dest => dest.LimitPrice, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.LimitPrice) ? (decimal?)null : Convert.ToDecimal(src.LimitPrice)))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ReverseMap();
        }
        private static int ConvertToInt(string value)
        {
            return int.TryParse(value, out var result) ? result : 0;
        }

        private static decimal? ConvertToDecimal(string? value)
        {
            return decimal.TryParse(value, out var result) ? result : (decimal?)null;
        }
    }
}
