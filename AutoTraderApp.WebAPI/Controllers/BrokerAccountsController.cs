using AutoTraderApp.Application.Features.BrokerAccounts.Commands.AddBrokerAccount;
using AutoTraderApp.Application.Features.BrokerAccounts.DTOs;
using AutoTraderApp.Application.Features.BrokerAccounts.Queries;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.ExternalModels.Alpaca.Models;
using AutoTraderApp.Infrastructure.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutoTraderApp.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrokerAccountsController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IAlpacaService _alpacaService;     

        public BrokerAccountsController(IMediator mediator, IAlpacaService alpacaService)
        {
            _mediator = mediator;
            _alpacaService = alpacaService;
        }

        /// <summary>
        /// Tüm broker hesaplarını getirir.
        /// </summary>
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAllBrokerAccountsQuery());
            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        /// <summary>
        /// broker hesabı bilgisi.
        /// </summary>
        [HttpGet("GetAccountInfo/{brokerAccountId}")]
        public async Task<IActionResult> GetAccountInfo(Guid brokerAccountId)
        {
            var result = await _mediator.Send(new GetAccountInfoQuery { brokerAccountId = brokerAccountId });
            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        /// <summary>
        /// Yeni bir broker hesabı ekler.
        /// </summary>
        [HttpPost("Add")]
        public async Task<IActionResult> Add([FromBody] AddBrokerAccountDto brokerAccountDto)
        {
            var command = new AddBrokerAccountCommand { BrokerAccountDto = brokerAccountDto };
            var result = await _mediator.Send(command);
            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpGet("alpaca-daily-trade-details")]
        public async Task<IActionResult> GetDailyTradeDetails([FromQuery] Guid brokerAccountId, [FromQuery] DateTime tradeDate)
        {
            try
            {
                var report = await _alpacaService.GenerateDailyTradeReportAsync(brokerAccountId, tradeDate);
                return Ok(new SuccessDataResult<string>(data: report, message: "Günlük işlem raporu başarıyla oluşturuldu."));

            }
            catch (Exception ex)
            {
                return BadRequest(new ErrorResult($"Hata: {ex.Message}"));
            }
        }

    }
}
