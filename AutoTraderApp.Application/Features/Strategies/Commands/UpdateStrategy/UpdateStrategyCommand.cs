using AutoMapper;
using AutoTraderApp.Application.Contracts.Repositories;
using AutoTraderApp.Application.Features.Strategies.DTOs;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Domain.Enums;
using MediatR;

namespace AutoTraderApp.Application.Features.Strategies.Commands.UpdateStrategy
{
    public class UpdateStrategyCommand : IRequest<IResult>
    {
        public Guid UserId { get; set; }
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public RiskLevel RiskLevel { get; set; }
        public decimal MaxPositionSize { get; set; }
        public decimal StopLossPercentage { get; set; }
        public decimal TakeProfitPercentage { get; set; }
        public List<TradingRuleDto> TradingRules { get; set; }

        public class UpdateStrategyCommandHandler : IRequestHandler<UpdateStrategyCommand, IResult>
        {
            private readonly IBaseRepository<Strategy> _strategyRepository;
            private readonly IBaseRepository<TradingRule> _tradingRuleRepository;
            private readonly IMapper _mapper;

            public UpdateStrategyCommandHandler(
                IBaseRepository<Strategy> strategyRepository,
                IBaseRepository<TradingRule> tradingRuleRepository,
                IMapper mapper)
            {
                _strategyRepository = strategyRepository;
                _tradingRuleRepository = tradingRuleRepository;
                _mapper = mapper;
            }

            public async Task<IResult> Handle(UpdateStrategyCommand request, CancellationToken cancellationToken)
            {
                var strategy = await _strategyRepository.GetAsync(s => s.Id == request.Id);
                if (strategy == null)
                    return new ErrorResult("Güncellenecek strateji bulunamadı.");

                var existingStrategy = await _strategyRepository.GetAsync(s => s.Name == request.Name && s.Id != request.Id);
                if (existingStrategy != null)
                    return new ErrorResult("Bu isimde başka bir strateji zaten mevcut.");


                var existingRules = await _tradingRuleRepository.GetListWithStringIncludeAsync(
                    r => r.StrategyId == request.Id);

                foreach (var rule in existingRules)
                {
                    await _tradingRuleRepository.DeleteAsync(rule);
                }

                _mapper.Map(request, strategy);
                await _strategyRepository.UpdateAsync(strategy);

                foreach (var ruleDto in request.TradingRules)
                {
                    var tradingRule = _mapper.Map<TradingRule>(ruleDto);
                    tradingRule.StrategyId = strategy.Id;
                    await _tradingRuleRepository.AddAsync(tradingRule);
                }

                return new SuccessResult("Alım-satım stratejisi başarıyla güncellendi.");
            }
        }
    }
}

