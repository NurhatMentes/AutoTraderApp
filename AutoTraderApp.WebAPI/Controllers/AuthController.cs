using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoTraderApp.Application.Features.Auth.Commands.Login;
using AutoTraderApp.Application.Features.Auth.Commands.RefreshToken;
using AutoTraderApp.Application.Features.Auth.Commands.Register;
using AutoTraderApp.Core.Constants;
using AutoTraderApp.Core.Utilities.Results;
using IResult = AutoTraderApp.Core.Utilities.Results.IResult;

namespace AutoTraderApp.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : BaseController
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            var result = await _mediator.Send(command);
            return ActionResultInstance(result);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterCommand command)
        {
            var result = await _mediator.Send(command);
            return ActionResultInstance(result);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
        {
            var result = await _mediator.Send(command);
            return ActionResultInstance((IResult)result);
        }

        [HttpGet("verify")]
        [Authorize]
        public IActionResult VerifyToken()
        {
            return ActionResultInstance(new SuccessResult(Messages.Auth.TokenCreated));
        }
    }
}
