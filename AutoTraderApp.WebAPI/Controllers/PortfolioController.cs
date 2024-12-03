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

        [HttpGet]
        public async Task<IActionResult> GetPortfolio()
        {
            var result = await _mediator.Send(new GetPortfolioQuery());
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }
}
