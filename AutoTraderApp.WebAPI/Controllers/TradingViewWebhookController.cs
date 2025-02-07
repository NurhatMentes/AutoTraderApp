using AutoTraderApp.Application.Features.TradingView.Commands.CryptoProcessTradingViewSignal;
using AutoTraderApp.Application.Features.TradingView.Commands.OkxTrProcessTradingViewSignal;
using AutoTraderApp.Application.Features.TradingView.Commands.StockProcessTradingViewSignal;
using AutoTraderApp.Application.Features.TradingView.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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
            var result = await _mediator.Send(new StockProcessTradingViewSignalCommand { Signal = signal });
            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpPost("crypto-binance")]
        public async Task<IActionResult> CryptoBinance([FromBody] TradingViewCryptoSignalDto signal)
        {
            var result = await _mediator.Send(new CryptoProcessTradingViewSignalCommand { Signal = signal });
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
