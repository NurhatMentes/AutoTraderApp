﻿using AutoTraderApp.Application.Features.TradingView.Commands.ProcessTradingViewSignal;
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

        [HttpPost("webhook")]
        public async Task<IActionResult> ReceiveSignal([FromBody] TradingViewSignalDto signal)
        {
            var result = await _mediator.Send(new ProcessTradingViewSignalCommand { Signal = signal });
            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }
    }
}
