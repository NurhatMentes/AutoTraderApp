using AutoTraderApp.Application.Features.TradingView.Commands.CryptoProcessTradingViewSignal;
using AutoTraderApp.Application.Features.TradingView.Commands.StockProcessTradingViewSignal;
using AutoTraderApp.Application.Features.TradingView.DTOs;
using AutoTraderApp.Domain.ExternalModels.TradingView;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoTraderApp.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TradingViewWebhookController : BaseController
    {
        private readonly IMediator _mediator;

        public TradingViewWebhookController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> ReceiveSignal([FromBody] TradingViewSignalDto signal)
        {
            var result = await _mediator.Send(new ProcessTradingViewSignalCommand { Signal = signal });
            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpPost("crypto-webhook")]
        public async Task<IActionResult> CryptoWebhook([FromBody] TradingViewCryptoSignalDto signal)
        {
            var result = await _mediator.Send(new CryptoProcessTradingViewSignalCommand { Signal = signal });
            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }
    }
}
