using AutoTraderApp.Application.Features.Orders.Commands.CancelOrder;
using AutoTraderApp.Application.Features.Orders.Commands.CreateOrder;
using AutoTraderApp.Application.Features.Orders.Commands.FillOrder;
using AutoTraderApp.Application.Features.Orders.Commands.RejectOrder;
using AutoTraderApp.Application.Features.Orders.Queries.GetOrderDetails;
using AutoTraderApp.Application.Features.Orders.Queries.GetUserOrders;
using AutoTraderApp.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand command)
        {
            command.UserId = GetUserId();
            return ActionResultInstance(await _mediator.Send(command));
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelOrder(Guid id)
        {
            var command = new CancelOrderCommand
            {
                OrderId = id,
                UserId = GetUserId()
            };
            return ActionResultInstance(await _mediator.Send(command));
        }

        [HttpPost("{id}/fill")]
        //[Authorize(Roles = "Admin")]  
        public async Task<IActionResult> FillOrder(Guid id, [FromBody] decimal executedPrice)
        {
            var command = new FillOrderCommand
            {
                OrderId = id,
                ExecutedPrice = executedPrice
            };
            return ActionResultInstance(await _mediator.Send(command));
        }

        [HttpGet]
        public async Task<IActionResult> GetUserOrders(
            [FromQuery] OrderStatus? status,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var query = new GetUserOrdersQuery
            {
                UserId = GetUserId(),
                Status = status,
                StartDate = startDate,
                EndDate = endDate,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            return ActionResultInstance(await _mediator.Send(query));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderDetails(Guid id)
        {
            var query = new GetOrderDetailsQuery
            {
                OrderId = id,
                UserId = GetUserId()
            };
            return ActionResultInstance(await _mediator.Send(query));
        }
    }
}
