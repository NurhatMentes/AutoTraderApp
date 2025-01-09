using AutoTraderApp.Application.Features.Position.Commands.ClosePosition;
using AutoTraderApp.Application.Features.Position.Queries;
using AutoTraderApp.Infrastructure.Interfaces;
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
        private readonly IAlpacaService _alpacaService;

        public PositionsController(IMediator mediator, IAlpacaService alpacaService)
        {
            _mediator = mediator;
            _alpacaService = alpacaService;
        }

        [HttpGet("alpaca/get-positions/{brokerAccountId}")]
        public async Task<IActionResult> GetPositions(Guid brokerAccountId)
        {
            var result = await _mediator.Send(new GetPositionsQuery { BrokerAccountId = brokerAccountId });
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("alpaca/get-open-positions/{brokerAccountId}")]
        public async Task<IActionResult> GetOpenPositions(Guid brokerAccountId)
        {
            var result = await _mediator.Send(new GetPositionsQuery { BrokerAccountId = brokerAccountId });
            return Ok(result);
        }

        [HttpPost("alpaca/close-position")]
        public async Task<IActionResult> ClosePosition([FromBody] ClosePositionCommand request)
        {
            var result = await _mediator.Send(request);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpPost("alpaca/all-close-position")]
        public async Task<IActionResult> AllClosePosition(Guid brokerAccountId)
        {
            var result = await _alpacaService.CloseAllPositionAsync(brokerAccountId);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }
    }
}
