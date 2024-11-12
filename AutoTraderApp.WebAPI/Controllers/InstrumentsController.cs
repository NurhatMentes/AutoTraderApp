using AutoTraderApp.Application.Features.Instruments.Commands.CreateInstrument;
using AutoTraderApp.Application.Features.Instruments.Queries.GetInstruments;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AutoTraderApp.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstrumentsController : BaseController
    {
        private readonly IMediator _mediator;

        public InstrumentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")] 
        public async Task<IActionResult> Create([FromBody] CreateInstrumentCommand command)
        {
            return ActionResultInstance(await _mediator.Send(command));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] GetInstrumentsQuery query)
        {
            return ActionResultInstance(await _mediator.Send(query));
        }
    }
}
