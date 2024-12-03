using AutoMapper;
using AutoTraderApp.Application.Features.MarketData.Alpaca.DTOs;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.ExternalModels.Alpaca.Models;
using AutoTraderApp.Infrastructure.Interfaces;
using MediatR;

namespace AutoTraderApp.Application.Features.MarketData.Alpaca.Queries
{
    public class GetMarketDataQuery : IRequest<IDataResult<List<MarketDataDto>>>
    {
        public string? Symbol { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 30;
    }

    public class GetMarketDataQueryHandler : IRequestHandler<GetMarketDataQuery, IDataResult<List<MarketDataDto>>>
    {
        private readonly IAlpacaService _alpacaService;
        private readonly IMapper _mapper;

        public GetMarketDataQueryHandler(IAlpacaService alpacaService, IMapper mapper)
        {
            _alpacaService = alpacaService;
            _mapper = mapper;
        }

        public async Task<IDataResult<List<MarketDataDto>>> Handle(GetMarketDataQuery request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(request.Symbol))
            {
                var marketDataResponse = await _alpacaService.GetMarketDataAsync(request.Symbol);
                var marketData = _mapper.Map<MarketDataDto>(marketDataResponse);
                return new SuccessDataResult<List<MarketDataDto>>(new List<MarketDataDto> { marketData }, "Piyasa verileri başarıyla alındı.");
            }

            var allMarketDataResponse = await _alpacaService.GetAllMarketDataAsync(request.Page, request.PageSize);
            var allMarketData = _mapper.Map<List<MarketDataDto>>(allMarketDataResponse);
            return new SuccessDataResult<List<MarketDataDto>>(allMarketData, "Tüm piyasa verileri başarıyla alındı.");
        }
    }


}
