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

        [HttpGet]
        public async Task<IActionResult> GetPositions()
        {
            var result = await _mediator.Send(new GetPositionsQuery());
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [HttpGet("open")]
        public async Task<IActionResult> GetOpenPositions()
        {
            var result = await _mediator.Send(new GetPositionsQuery());
            return Ok(result);
        }

        [HttpPost("close")]
        public async Task<IActionResult> ClosePosition([FromBody] ClosePositionCommand request)
        {
            var result = await _mediator.Send(request);
            return Ok(result);
        }
    }
}
