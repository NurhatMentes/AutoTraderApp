using AutoTraderApp.Application.Features.Portfolio.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutoTraderApp.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PortfolioController : BaseController
    {
        private readonly IMediator _mediator;

        public PortfolioController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("alpaca/get_portfolio/{brokerAccountId}")]
        public async Task<IActionResult> GetPortfolio(Guid brokerAccountId)
        {
            var result = await _mediator.Send(new GetPortfolioQuery { BrokerAccountId = brokerAccountId });
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }
}
