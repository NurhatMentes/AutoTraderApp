using AutoTraderApp.Application.Features.Prices.Commands.AddBulkPrice;
using AutoTraderApp.Application.Features.Prices.Commands.AddPrice;
using AutoTraderApp.Application.Features.Prices.Queries.GetPrices;
using AutoTraderApp.Application.Features.Prices.Queries.GetPricesByTimeframe;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutoTraderApp.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PricesController : BaseController
    {
        private readonly IMediator _mediator;

        public PricesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        //[Authorize(Roles = "Admin")] 
        public async Task<IActionResult> AddPrice([FromBody] AddPriceCommand command)
        {
            return ActionResultInstance(await _mediator.Send(command));
        }

        [HttpPost("bulk")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddBulkPrice([FromBody] AddBulkPriceCommand command)
        {
            return ActionResultInstance(await _mediator.Send(command));
        }

        [HttpGet("{instrumentId}")]
        public async Task<IActionResult> GetPrices(
            Guid instrumentId,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int? limit)
        {
            var query = new GetPricesQuery
            {
                InstrumentId = instrumentId,
                StartDate = startDate,
                EndDate = endDate,
                Limit = limit
            };
            return ActionResultInstance(await _mediator.Send(query));
        }

        [HttpGet("{instrumentId}/timeframe")]
        public async Task<IActionResult> GetPricesByTimeframe(
            Guid instrumentId,
            [FromQuery] TimeFrame timeFrame,
            [FromQuery] int count = 100)
        {
            var query = new GetPricesByTimeframeQuery
            {
                InstrumentId = instrumentId,
                TimeFrame = timeFrame,
                Count = count
            };
            return ActionResultInstance(await _mediator.Send(query));
        }
    }
}
