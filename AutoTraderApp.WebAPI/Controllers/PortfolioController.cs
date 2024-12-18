using AutoTraderApp.Application.Features.Portfolio.Queries;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Infrastructure.Interfaces;
using AutoTraderApp.Infrastructure.Services.Alpaca;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace AutoTraderApp.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PortfolioController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IAlpacaService _alpacaService;

        public PortfolioController(IMediator mediator, IAlpacaService alpacaService)
        {
            _mediator = mediator;
            _alpacaService = alpacaService;
        }

        [HttpGet("alpaca/get_portfolio/{brokerAccountId}")]
        public async Task<IActionResult> GetPortfolio(Guid brokerAccountId)
        {
            var result = await _mediator.Send(new GetPortfolioQuery { BrokerAccountId = brokerAccountId });
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("alpaca/calculate_daily_pnl/{brokerAccountId}")]
        public async Task<IActionResult> CalculateDailyPnl(Guid brokerAccountId, DateTime startDate, DateTime endDate)
        {

            var orders = await _alpacaService.GetFilledOrdersAsync(brokerAccountId, startDate, endDate);
            var pnlResults = await _alpacaService.CalculateDailyPnL(orders);

            if (pnlResults != null)
                return Ok(new SuccessDataResult<Dictionary<string, string>>(pnlResults, "Günlük kar/zarar hesaplandı."));

            return BadRequest(new ErrorResult("Bır sournla karşılaşıldı."));

        }


    }
}
