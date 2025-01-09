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
        private readonly ITradingViewSeleniumService _tradingViewSeleniumService;

        public UserTradingAccountsController(IMediator mediator, ITradingViewAutomationService tradingViewAutomationService, ITradingViewSeleniumService tradingViewSeleniumService)
        {
            _mediator = mediator;
            _tradingViewAutomationService = tradingViewAutomationService;
            _tradingViewSeleniumService = tradingViewSeleniumService;
        }

        [HttpPost("CreateTradingAccount")]
        public async Task<IActionResult> CreateTradingAccount([FromBody] CreateUserTradingAccountCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpPost("playwright-login")]
        public async Task<IActionResult> LoginFromPlaywrightToTradingView([FromBody] TradingViewLoginDto loginDto)
        {
            var result = await _tradingViewAutomationService.LoginAsync(loginDto.UserId, loginDto.Password);
            if (result)
            {
                return Ok(new { Message = "TradingView'e giriş başarılı." });
            }

            return BadRequest(new { Message = "TradingView'e giriş başarısız." });
        }

        [HttpPost("selenium-login-sync")]
        public async Task<IActionResult> LoginFromSeleniumToTradingView([FromBody] TradingViewLoginDto loginDto)
        {
            var result =  _tradingViewSeleniumService.Login(loginDto.UserId, loginDto.Password);
            if (result)
            {
                return Ok(new { Message = "TradingView'e giriş başarılı." });
            }

            return BadRequest(new { Message = "TradingView'e giriş başarısız." });
        }
    }
}
