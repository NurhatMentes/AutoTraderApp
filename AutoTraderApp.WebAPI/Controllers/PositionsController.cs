using AutoTraderApp.Application.Features.Positions.Commands.ClosePosition;
using AutoTraderApp.Application.Features.Positions.Queries.GetUserPositions;
using AutoTraderApp.Domain.Enums;
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

        [HttpPost("{id}/close")]
        public async Task<IActionResult> ClosePosition(
            Guid id,
            [FromBody] decimal? closePrice = null)
        {
            var command = new ClosePositionCommand
            {
                PositionId = id,
                UserId = GetUserId(),
                ClosePrice = closePrice
            };
            return ActionResultInstance(await _mediator.Send(command));
        }

        [HttpGet]
        public async Task<IActionResult> GetPositions(
            [FromQuery] PositionStatus? status,
            [FromQuery] PositionSide? side,
            [FromQuery] Guid? instrumentId)
        {
            var query = new GetUserPositionsQuery
            {
                UserId = GetUserId(),
                Status = status,
                Side = side,
                InstrumentId = instrumentId
            };
            return ActionResultInstance(await _mediator.Send(query));
        }
    }
}
