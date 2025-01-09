using AutoTraderApp.Application.Features.CombinedStocks.Commands;
using AutoTraderApp.Application.Features.CombinedStocks.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutoTraderApp.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CombinedStockController : BaseController
    {
        private readonly IMediator _mediator;

        public CombinedStockController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("update_combined_stock")]
        public async Task<IActionResult> UpdateCombinedStockList()
        {
            var result = await _mediator.Send(new UpdateCombinedStockListCommand());
            if (result)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpGet("combined_stock_list")]
        public async Task<IActionResult> GetCombinedStocks()
        {
            var result = await _mediator.Send(new GetCombinedStocksQuery());
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result.Message);
        }

    }
}
