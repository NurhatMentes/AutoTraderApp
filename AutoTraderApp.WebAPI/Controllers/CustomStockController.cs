using AutoTraderApp.Application.Features.CustomStocks.Commands;
using AutoTraderApp.Application.Features.CustomStocks.DTOs;
using AutoTraderApp.Application.Features.CustomStocks.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutoTraderApp.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomStockController : BaseController
    {
        private readonly IMediator _mediator;

        public CustomStockController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<List<CustomStockDto>>> GetAll()
        {
            var stocks = await _mediator.Send(new GetAllCustomStocksQuery());
            return Ok(stocks);
        }

        [HttpPost]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateCustomStockDto dto)
        {
            var command = new CreateCustomStockCommand { Dto = dto };
            var id = await _mediator.Send(command);
            return Ok(id);
        }

        [HttpPut]
        public async Task<ActionResult> Update([FromBody] UpdateCustomStockDto dto)
        {
            var command = new UpdateCustomStockCommand { Dto = dto };
            await _mediator.Send(command);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(Guid id)
        {
            var command = new DeleteCustomStockCommand { Id = id };
            await _mediator.Send(command);
            return NoContent();
        }
    }
}