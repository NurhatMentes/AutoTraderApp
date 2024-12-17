using AutoTraderApp.Application.Features.Position.Commands.ClosePosition;
using AutoTraderApp.Application.Features.Position.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutoTraderApp.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PositionsController : BaseController
    {
        private readonly IMediator _mediator;

        public PositionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("alpaca/get_positions/{brokerAccountId}")]
        public async Task<IActionResult> GetPositions(Guid brokerAccountId)
        {
            var result = await _mediator.Send(new GetPositionsQuery { BrokerAccountId = brokerAccountId });
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("alpaca/get_open_positions/{brokerAccountId}")]
        public async Task<IActionResult> GetOpenPositions(Guid brokerAccountId)
        {
            var result = await _mediator.Send(new GetPositionsQuery { BrokerAccountId = brokerAccountId });
            return Ok(result);
        }

        [HttpPost("alpaca/get_close_positions/{brokerAccountId}")]
        public async Task<IActionResult> ClosePosition([FromBody] ClosePositionCommand request)
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }
    }
}
