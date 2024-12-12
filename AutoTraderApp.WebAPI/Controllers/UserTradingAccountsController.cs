using AutoTraderApp.Application.Features.UserTradingAccounts.Commands.CreateUserTradingAccount;
using AutoTraderApp.Application.Features.UserTradingAccounts.DTOs;
using AutoTraderApp.Infrastructure.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutoTraderApp.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserTradingAccountsController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly ITradingViewAutomationService _tradingViewAutomationService;

        public UserTradingAccountsController(IMediator mediator, ITradingViewAutomationService tradingViewAutomationService)
        {
            _mediator = mediator;
            _tradingViewAutomationService = tradingViewAutomationService;
        }

        [HttpPost("CreateTradingAccount")]
        public async Task<IActionResult> CreateTradingAccount([FromBody] CreateUserTradingAccountCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginToTradingView([FromBody] TradingViewLoginDto loginDto)
        {
            var result = await _tradingViewAutomationService.LoginAsync(loginDto.UserId, loginDto.Password);
            if (result)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

    }
}
