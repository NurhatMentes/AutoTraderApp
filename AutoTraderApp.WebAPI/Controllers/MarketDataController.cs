using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Infrastructure.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutoTraderApp.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarketDataController : BaseController
    {
        private readonly IMarketDataService _marketDataService;
        private readonly IMediator _mediator;


        public MarketDataController(IMarketDataService marketDataService, IMediator mediator)
        {
            _marketDataService = marketDataService;
            _mediator = mediator;
        }

        [HttpGet("alphaVantage/price/{symbol}")]
        public async Task<IActionResult> AlphaVantageGetCurrentPrice(string symbol)
        {
            var price = await _marketDataService.GetCurrentPrice(symbol);
            if (!price.HasValue)
                return NotFound($"Sembol için fiyat bulunamadı: {symbol}");

            return Ok(new SuccessDataResult<decimal>(price.Value, $"Şu anki fiyat {symbol}"));
        }

        [HttpGet("alphaVantage/historical/{symbol}")]
        public async Task<IActionResult> AlphaVantageGetHistoricalPrices(
            string symbol,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var prices = await _marketDataService.GetHistoricalPrices(symbol, startDate, endDate);
            return Ok(new SuccessDataResult<IEnumerable<Price>>(prices, $"Geçmiş fiyatlar {symbol}"));
        }

        [HttpGet("alphaVantage/intraday/{symbol}")]
        public async Task<IActionResult> AlphaVantageGetIntradayPrices(
            string symbol,
            [FromQuery] string interval = "5min")
        {
            var intradayPrices = await _marketDataService.GetIntraday(symbol, interval);

            if (intradayPrices == null || !intradayPrices.Any())
                return NotFound($"Sembol için gün içi verisi bulunamadı: {symbol}");

            return Ok(new SuccessDataResult<IEnumerable<Price>>(intradayPrices, $"Gün içi fiyatlar {symbol} ({interval})"));
        }
        
    }
}
