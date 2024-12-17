using AutoMapper;
using AutoTraderApp.Application.Features.Position.DTOs;
using AutoTraderApp.Domain.Entities;

public class PositionProfile : Profile
{
    public PositionProfile()
    {
        CreateMap<PositionResponse, PositionDto>()
           .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => Convert.ToDecimal(src.Quantity)))
           .ForMember(dest => dest.MarketValue, opt => opt.MapFrom(src => Convert.ToDecimal(src.MarketValue)))
           .ForMember(dest => dest.CostBasis, opt => opt.MapFrom(src => Convert.ToDecimal(src.CostBasis)))
           .ForMember(dest => dest.UnrealizedPnL, opt => opt.MapFrom(src => Convert.ToDecimal(src.UnrealizedPnL)))
           .ForMember(dest => dest.UnrealizedPnLPercentage, opt => opt.MapFrom(src => Convert.ToDecimal(src.UnrealizedPnLPercentage)))
           .ForMember(dest => dest.CurrentPrice, opt => opt.MapFrom(src => Convert.ToDecimal(src.CurrentPrice)))
           .ForMember(dest => dest.RealizedPnL, opt => opt.MapFrom(src => Convert.ToDecimal(src.RealizedPnL)))
           .ForMember(dest => dest.TodayChange, opt => opt.MapFrom(src => Convert.ToDecimal(src.TodayChange)))
           .ForMember(dest => dest.EntryPrice, opt => opt.MapFrom(src => Convert.ToDecimal(src.EntryPrice)))
           .ForMember(dest => dest.AvailableQuantity, opt => opt.MapFrom(src => Convert.ToDecimal(src.AvailableQuantity)));

        CreateMap<Position, PositionDto>()
            .ForMember(dest => dest.BrokerAccountId, opt => opt.MapFrom(src => src.BrokerAccountId))
            .ForMember(dest => dest.Symbol, opt => opt.MapFrom(src => src.Symbol))
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => Convert.ToDecimal(src.Quantity)))
            .ForMember(dest => dest.EntryPrice, opt => opt.MapFrom(src => Convert.ToDecimal(src.EntryPrice)))
            .ForMember(dest => dest.MarketValue, opt => opt.MapFrom(src => Convert.ToDecimal(src.MarketValue)))
            .ForMember(dest => dest.CostBasis, opt => opt.MapFrom(src => Convert.ToDecimal(src.CostBasis)))
            .ForMember(dest => dest.UnrealizedPnL, opt => opt.MapFrom(src => Convert.ToDecimal(src.UnrealizedPnL)))
            .ForMember(dest => dest.UnrealizedPnLPercentage, opt => opt.MapFrom(src => Convert.ToDecimal(src.UnrealizedPnLPercentage)))
            .ForMember(dest => dest.CurrentPrice, opt => opt.MapFrom(src => Convert.ToDecimal(src.CurrentPrice)))
            .ForMember(dest => dest.RealizedPnL, opt => opt.MapFrom(src => Convert.ToDecimal(src.RealizedPnL)))
            .ForMember(dest => dest.TodayChange, opt => opt.MapFrom(src => Convert.ToDecimal(src.TodayChange)))
            .ForMember(dest => dest.IsOpen, opt => opt.MapFrom(src => true)).ReverseMap();

        CreateMap<ClosedPosition, ClosedPositionDto>().ForMember(dest => dest.BrokerAccountId, opt => opt.MapFrom(src => src.BrokerAccountId)).ReverseMap();
    }
}
