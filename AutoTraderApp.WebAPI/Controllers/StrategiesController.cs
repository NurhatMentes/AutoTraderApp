using AutoTraderApp.Application.Features.Strategies.Commands.CreateStrategy;
using AutoTraderApp.Application.Features.Strategies.Commands.DeleteStrategy;
using AutoTraderApp.Application.Features.Strategies.Commands.UpdateStrategy;
using AutoTraderApp.Application.Features.Strategies.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutoTraderApp.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize]
    public class StrategiesController : BaseController
    {
        private readonly IMediator _mediator;

        public StrategiesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CreateStrategyCommand createStrategyCommand)
        {
            createStrategyCommand.UserId = GetUserId(); 
            var result = await _mediator.Send(createStrategyCommand);
            return ActionResultInstance(result);
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateStrategyCommand updateStrategyCommand)
        {
            updateStrategyCommand.UserId = GetUserId(); 
            var result = await _mediator.Send(updateStrategyCommand);
            return ActionResultInstance(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            DeleteStrategyCommand deleteStrategyCommand = new() { Id = id };
            var result = await _mediator.Send(deleteStrategyCommand);
            return ActionResultInstance(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetStrategiesQuery());
            return ActionResultInstance(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            var result = await _mediator.Send(new GetStrategyDetailQuery { Id = id });
            return ActionResultInstance(result);
        }
    }
}
