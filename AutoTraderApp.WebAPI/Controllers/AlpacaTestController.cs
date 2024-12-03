using AutoTraderApp.Infrastructure.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AutoTraderApp.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlpacaTestController : ControllerBase
    {
        private readonly IAlpacaService _alpacaService;
        private readonly IMediator _mediator;


        public AlpacaTestController(IAlpacaService alpacaService, IMediator mediator)
        {
            _alpacaService = alpacaService;
            _mediator = mediator;
        }



        [HttpGet("TestAlpacaApi")]
        public async Task<IActionResult> TestAlpacaApi()
        {
            using var client = new HttpClient
            {
                BaseAddress = new Uri("https://paper-api.alpaca.markets/")
            };
            client.DefaultRequestHeaders.Add("APCA-API-KEY-ID", "PKE5PM9X7GXO6FLHW5SL");
            client.DefaultRequestHeaders.Add("APCA-API-SECRET-KEY", "sxI7rh3pYs5CtlyF6JbNaUGbh9f9s3tkJebBZN2O");

            var response = await client.GetAsync("v2/account");
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return BadRequest($"Error: {response.StatusCode} - {content}");
            }

            return Ok(content);
        }

    }
}
