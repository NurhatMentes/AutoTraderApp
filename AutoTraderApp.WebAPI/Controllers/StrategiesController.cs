using AutoTraderApp.Application.Features.Strategies.Commands.CreateTradingViewStrategyById;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AutoTraderApp.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StrategiesController : BaseController
    {
        private readonly IBaseRepository<Strategy> _strategyRepository;
        private readonly IMediator _mediator;

        public StrategiesController(IMediator mediator, IBaseRepository<Strategy> strategyRepository)
        {
            _strategyRepository = strategyRepository;
            _mediator = mediator;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllStrategies()
        {
            var strategies = await _strategyRepository.GetAllAsync();
            return Ok(strategies);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateStrategy(Guid strategyId, Guid brokerAccountId, Guid userId)
        {
            var result = await _mediator.Send(new CreateTradingViewStrategyByIdCommand { StrategyId = strategyId, BrokerAccountId = brokerAccountId, UserId = userId });
            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }
    }
}
