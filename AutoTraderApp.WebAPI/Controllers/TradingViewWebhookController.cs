using AutoTraderApp.Application.Features.TradingView.Commands.AlpacaProcessTradingViewSignal;
using AutoTraderApp.Application.Features.TradingView.Commands.BinanceProcessTradingViewSignal;
using AutoTraderApp.Application.Features.TradingView.Commands.OkxTrProcessTradingViewSignal;
using AutoTraderApp.Application.Features.TradingView.DTOs;
using MediatR;
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

        [HttpPost("stock-alpaca")]
        public async Task<IActionResult> StockAlpaca([FromBody] TradingViewSignalDto signal)
        {
            var result = await _mediator.Send(new AlpacaProcessTradingViewSignalCommand { Signal = signal });
            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpPost("crypto-binance")]
        public async Task<IActionResult> CryptoBinance([FromBody] TradingViewCryptoSignalDto signal)
        {
            var result = await _mediator.Send(new BinanceProcessTradingViewSignalCommand { Signal = signal });
            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpPost("crypto-okx-tr")]
        public async Task<IActionResult> CryptoOkxTr([FromBody] TradingViewCryptoSignalDto signal)
        {
            var result = await _mediator.Send(new OkxTrProcessTradingViewSignalCommand { Signal = signal });
            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }
    }
}
