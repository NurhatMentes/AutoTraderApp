using AutoTraderApp.Application.Features.Orders.Commands.PlaceOrder;
using AutoTraderApp.Application.Features.Orders.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AutoTraderApp.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : BaseController
    {
        private readonly IMediator _mediator;

        public OrdersController(IMediator mediator)
        {
            _mediator = mediator;
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


    }
}