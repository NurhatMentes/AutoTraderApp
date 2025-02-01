using AutoTraderApp.Application.Features.CryptoCustomStocks.Commands;
using AutoTraderApp.Application.Features.CryptoCustomStocks.DTOs;
using AutoTraderApp.Application.Features.CryptoCustomStocks.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutoTraderApp.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CryptoCustomStockController : BaseController
    {
        private readonly IMediator _mediator;

        public CryptoCustomStockController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAllCryptoCustomStocksQuery());
            return Ok(result);
        }

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetCryptoCustomStockByIdQuery { Id = id });

            if (result == null)
                return NotFound("CryptoCustomStock bulunamadı");

            return Ok(result);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> Create([FromBody] CreateCryptoCustomStockDto dto)
        {
            var command = new CreateCryptoCustomStockCommand { Dto = dto };
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result }, result);
        }

        [HttpPut("Update")]
        public async Task<IActionResult> Update([FromBody] UpdateCryptoCustomStockDto dto)
        {
            var command = new UpdateCryptoCustomStockCommand { Dto = dto };
            var result = await _mediator.Send(command);

            if (!result)
                return NotFound("Güncelleme başarısız oldu. CryptoCustomStock bulunamadı.");

            return Ok("CryptoCustomStock başarıyla güncellendi");
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new DeleteCryptoCustomStockCommand { Id = id };
            var result = await _mediator.Send(command);

            if (!result)
                return NotFound("Silinemedi. CryptoCustomStock bulunamadı.");

            return Ok("CryptoCustomStock başarıyla silindi");
        }
    }
}
