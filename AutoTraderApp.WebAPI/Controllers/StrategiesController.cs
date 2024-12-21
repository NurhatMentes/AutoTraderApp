using AutoTraderApp.Application.Features.Strategies.Commands.ApplyStrategyToMultipleStocks;
using AutoTraderApp.Application.Features.Strategies.Commands.CreateTradingViewStrategyById;
using AutoTraderApp.Application.Features.Strategies.DTOs;
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

        [HttpGet("get-all-strategy")]
        public async Task<IActionResult> GetAllStrategies()
        {
            var strategies = await _strategyRepository.GetAllAsync();
            return Ok(strategies);
        }

        [HttpPost("create-strategy")]
        public async Task<IActionResult> CreateStrategy(Guid strategyId, Guid brokerAccountId, Guid userId)
        {
            var result = await _mediator.Send(new CreateTradingViewStrategyByIdCommand { StrategyId = strategyId, BrokerAccountId = brokerAccountId, UserId = userId });
            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpPost("apply-strategy-to-multiple-stocks")]
        public async Task<IActionResult> ApplyStrategyToMultipleStocks(Guid strategyId, Guid brokerAccountId, Guid userId)
        {
            var result = await _mediator.Send(new ApplyStrategyToMultipleStocksCommand
            {
                StrategyId = strategyId,
                BrokerAccountId = brokerAccountId,
                UserId = userId
            });

            if (result.Success)
                return Ok(result);

            return BadRequest(result);
        }

    }
}
