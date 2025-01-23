using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Domain.ExternalModels.AlphaVantage;
using AutoTraderApp.Infrastructure.Interfaces;
using AutoTraderApp.Infrastructure.Services.Alpaca;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutoTraderApp.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MarketDataController : BaseController
    {
        private readonly IAlphaVantageService _marketDataService;
        private readonly IMediator _mediator;
        private readonly IAlpacaService _alpacaService;
        private readonly IPolygonService _polygonService;


        public MarketDataController(IAlphaVantageService marketDataService, IMediator mediator, IAlpacaService alpacaService, IPolygonService polygonService)
        {
            _marketDataService = marketDataService;
            _mediator = mediator;
            _alpacaService = alpacaService;
            _polygonService = polygonService;
        }

        [HttpGet("polygon/stock/{symbol}")]
        public async Task<IActionResult> PolygonGetStock(string symbol)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                return BadRequest("Geçerli bir sembol belirtiniz.");
            }

            var result = await _polygonService.GetStockPriceAsync(symbol);
            return Ok(result);
        }

        [HttpGet("alpaca-latest-price")]
        public async Task<IActionResult> AlpacaGetLatestPrice([FromQuery] string symbol, [FromQuery] Guid brokerAccountId)
        {
            if (string.IsNullOrWhiteSpace(symbol))
            {
                return BadRequest("Geçerli bir sembol belirtiniz.");
            }

            var latestPrice = await _alpacaService.GetLatestPriceAsync(symbol.ToUpperInvariant(), brokerAccountId);
            return Ok(latestPrice);


        }

        [HttpGet("alphaVantage/price/{symbol}")]
        public async Task<IActionResult> AlphaVantageGetCurrentPrice(string symbol)
        {
            var price = await _marketDataService.GetCurrentPrice(symbol);
            if (price == null)
                return NotFound($"Sembol için fiyat bulunamadı: {symbol}");

            return Ok(new SuccessResult($"Şu anki fiyat {price}"));
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

        [HttpGet("alphaVantage/top_gainers")]
        public async Task<IActionResult> GetTopGainers()
        {
            var gainers = await _marketDataService.GetTopGainersAsync();
            return Ok(gainers);
        }

        [HttpGet("alphaVantage/top_losers")]
        public async Task<IActionResult> GetTopLosers()
        {
            var losers = await _marketDataService.GetTopLosersAsync();
            return Ok(losers);
        }

        [HttpGet("alphaVantage/most_activite")]
        public async Task<IActionResult> GetMostActive()
        {
            var actives = await _marketDataService.GetMostActiveAsync();
            return Ok(actives);
        }

        [HttpGet("alphaVantage/nasdaq-listings")]
        public async Task<IActionResult> GetNasdaqListings([FromQuery] int? limit = null)
        {
            try
            {
                var listings = await _marketDataService.GetNasdaqListingsAsync(limit);
                return Ok(new SuccessDataResult<List<StockListingDto>>(
                    listings,
                    limit.HasValue
                        ? $"İlk {limit} NASDAQ hissesi listelendi"
                        : "Tüm NASDAQ hisseleri listelendi"
                ));
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResult(ex.Message));
            }
        }

        [HttpGet("alphaVantage/nasdaq-listings/{count}")]
        public async Task<IActionResult> GetLimitedNasdaqListings(int count)
        {
            if (count <= 0 || count > 1000)
            {
                return BadRequest(new ErrorResult("Hisse sayısı 1 ile 1000 arasında olmalıdır."));
            }

            try
            {
                var listings = await _marketDataService.GetNasdaqListingsAsync(count);
                return Ok(new SuccessDataResult<List<StockListingDto>>(
                    listings,
                    $"NASDAQ'tan {count} hisse listelendi"
                ));
            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResult(ex.Message));
            }
        }
    }
}
