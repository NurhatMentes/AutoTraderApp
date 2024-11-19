using AutoMapper;
using AutoTraderApp.Application.Contracts.Repositories;
using AutoTraderApp.Application.Features.Orders.Commands.CreateOrder;
using AutoTraderApp.Application.Features.Strategies.DTOs;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutoTraderApp.Application.Features.Strategies.Commands.CreateStrategy
{
    public class CreateStrategyCommand : IRequest<IResult>
    {
        public Guid UserId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public RiskLevel RiskLevel { get; set; }
        public decimal MaxPositionSize { get; set; }
        public decimal StopLossPercentage { get; set; }
        public decimal TakeProfitPercentage { get; set; }
        public List<TradingRuleDto> TradingRules { get; set; }

        public class CreateStrategyCommandHandler : IRequestHandler<CreateStrategyCommand, IResult>
        {
            private readonly IBaseRepository<Strategy> _strategyRepository;
            private readonly IBaseRepository<User> _userRepository;
            private readonly IBaseRepository<TradingRule> _tradingRuleRepository;
            private readonly IMapper _mapper;

            public CreateStrategyCommandHandler(
                IBaseRepository<Strategy> strategyRepository,
                IBaseRepository<User> userRepository,
                IBaseRepository<TradingRule> tradingRuleRepository,
                IMapper mapper)
            {
                _strategyRepository = strategyRepository;
                _userRepository = userRepository;
                _tradingRuleRepository = tradingRuleRepository;
                _mapper = mapper;
            }

            public async Task<IResult> Handle(CreateStrategyCommand request, CancellationToken cancellationToken)
            {
                // Önce kullanıcının varlığını kontrol et
                var user = await _userRepository.GetByIdAsync(request.UserId);
                if (user == null)
                    return new ErrorResult("Belirtilen kullanıcı bulunamadı.");

                var strategy = _mapper.Map<Strategy>(request);
                strategy.Status = StrategyStatus.Active;
                strategy.CreatedByUserId = request.UserId;  // BaseEntity'den gelen alan

                var addedStrategy = await _strategyRepository.AddAsync(strategy);

                foreach (var ruleDto in request.TradingRules)
                {
                    var tradingRule = _mapper.Map<TradingRule>(ruleDto);
                    tradingRule.StrategyId = addedStrategy.Id;
                    tradingRule.CreatedByUserId = request.UserId;  // BaseEntity'den gelen alan
                    await _tradingRuleRepository.AddAsync(tradingRule);
                }

                return new SuccessResult("Alım-satım stratejisi başarıyla oluşturuldu.");
            }
        }
    }
}
