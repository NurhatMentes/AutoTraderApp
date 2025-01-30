using AutoTraderApp.Application.Features.UserTradingSettings.Commands;
using AutoTraderApp.Application.Features.UserTradingSettings.DTOs;
using AutoTraderApp.Application.Features.UserTradingSettings.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutoTraderApp.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserTradingSettingsController : BaseController
    {
        private readonly IMediator _mediator;

        public UserTradingSettingsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{userId}")]
        public async Task<ActionResult<UserTradingSettingsDto>> GetSettings(Guid userId)
        {
            var query = new GetUserTradingSettingsQuery { UserId = userId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult> CreateSettings([FromBody] CreateUserTradingSettingsCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateSettings([FromBody] UpdateUserTradingSettingsCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
