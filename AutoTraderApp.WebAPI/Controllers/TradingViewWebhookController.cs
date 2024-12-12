using AutoTraderApp.Application.Features.TradingView.Commands.ProcessTradingViewSignal;
using AutoTraderApp.Application.Features.TradingView.Commands.SendStrategy;
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

        [HttpPost("send-strategy")]
        public async Task<IActionResult> SendStrategy([FromBody] TradingViewStrategyDto strategy)
        {
            var command = new SendTradingViewStrategyCommand { Strategy = strategy };
            var result = await _mediator.Send(command);

            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpPost("signal")]
        public IActionResult ReceiveSignal([FromBody] SignalRequest signalRequest)
        {
            // Verilerin doğruluğunu kontrol et
            if (string.IsNullOrWhiteSpace(signalRequest.Action) ||
                string.IsNullOrWhiteSpace(signalRequest.Symbol))
            {
                return BadRequest(new { Message = "Invalid signal data." });
            }

            // İşlemin başarılı olduğunu bildir
            return Ok(new
            {
                Status = "Signal received",
                Data = signalRequest
            });
        }
    }
}
