using AutoTraderApp.Application.Features.Orders.Commands.PlaceOrder;
using AutoTraderApp.Application.Features.Orders.DTOs;
using AutoTraderApp.Application.Features.Position.Commands.ClosePosition;
using AutoTraderApp.Application.Features.Position.Queries;
using AutoTraderApp.Domain.ExternalModels.Alpaca.Models;
using AutoTraderApp.Infrastructure.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AutoTraderApp.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : BaseController
    {
        private readonly IMediator _mediator;
        private readonly IAlpacaService _alpacaService;

        public OrdersController(IMediator mediator, IAlpacaService alpacaService)
        {
            _mediator = mediator;
            _alpacaService = alpacaService;
        }

        [HttpPost("PlaceOrder")]
        public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderDto orderDto)
        {
            var validator = new PlaceOrderValidator();
            var validationResult = validator.Validate(orderDto);

            if (!validationResult.IsValid)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Validasyon hataları.",
                    errors = validationResult.Errors.Select(e => e.ErrorMessage)
                });
            }

            var command = new PlaceOrderCommand { OrderDto = orderDto };
            var result = await _mediator.Send(command);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    success = false,
                    message = result.Message
                });
            }

            return Ok(new
            {
                success = true,
                message = result.Message
            });
        }

        [HttpGet("alpaca/get-orders/{brokerAccountId}")]
        public async Task<IActionResult> GetOrders(Guid brokerAccountId)
        {
            var result = await _alpacaService.GetAllOrdersAsync(brokerAccountId);
            if (result != null)
                return Ok(result);
            return BadRequest(result);
        }
    }
}