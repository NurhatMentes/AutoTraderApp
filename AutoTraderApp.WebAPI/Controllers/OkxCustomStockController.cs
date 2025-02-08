using AutoTraderApp.Application.Features.OkxCustomStocks.Commands;
using AutoTraderApp.Application.Features.OkxCustomStocks.DTOs;
using AutoTraderApp.Application.Features.OkxCustomStocks.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AutoTraderApp.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OkxCustomStockController : BaseController
    {
        private readonly IMediator _mediator;

        public OkxCustomStockController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAllOkxCustomStocksQuery());
            return Ok(result);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CreateOkxCustomStockDto dto)
        {
            var command = new CreateOkxCustomStockCommand { Dto = dto };
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpPut("Update")]
        public async Task<IActionResult> Update([FromBody] UpdateOkxCustomStockDto dto)
        {
            var command = new UpdateOkxCustomStockCommand { Dto = dto };
            var result = await _mediator.Send(command);

            if (!result)
                return NotFound("Güncelleme başarısız oldu. OkxCustomStock bulunamadı.");

            return Ok("OkxCustomStock başarıyla güncellendi");
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new DeleteOkxCustomStockCommand { Id = id };
            var result = await _mediator.Send(command);

            if (result == null)
                return NotFound("Silinemedi. OkxCustomStock bulunamadı.");

            return Ok("CryptoCustomStock başarıyla silindi");
        }
    }
}
