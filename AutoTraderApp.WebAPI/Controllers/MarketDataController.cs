using AutoTraderApp.Application.Interfaces;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutoTraderApp.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarketDataController : BaseController
    {
        private readonly IMarketDataService _marketDataService;

        public MarketDataController(IMarketDataService marketDataService)
        {
            _marketDataService = marketDataService;
        }

        [HttpGet("price/{symbol}")]
        public async Task<IActionResult> GetCurrentPrice(string symbol)
        {
            var price = await _marketDataService.GetCurrentPrice(symbol);
            if (!price.HasValue)
                return NotFound($"Sembol için fiyat bulunamadı: {symbol}");

            return Ok(new SuccessDataResult<decimal>(price.Value, $"Şu anki fiyat {symbol}"));
        }

        [HttpGet("historical/{symbol}")]
        public async Task<IActionResult> GetHistoricalPrices(
            string symbol,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var prices = await _marketDataService.GetHistoricalPrices(symbol, startDate, endDate);
            return Ok(new SuccessDataResult<IEnumerable<Price>>(prices, $"Geçmiş fiyatlar {symbol}"));
        }

        [HttpGet("intraday/{symbol}")]
        public async Task<IActionResult> GetIntradayPrices(
            string symbol,
            [FromQuery] string interval = "5min")
        {
            var intradayPrices = await _marketDataService.GetIntraday(symbol, interval);

            if (intradayPrices == null || !intradayPrices.Any())
                return NotFound($"Sembol için gün içi verisi bulunamadı: {symbol}");

            return Ok(new SuccessDataResult<IEnumerable<Price>>(intradayPrices, $"Gün i çi fiyatlar {symbol} ({interval})"));
        }
    }
}
