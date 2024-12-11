using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Infrastructure.Interfaces;
using MediatR;

namespace AutoTraderApp.Application.Features.Strategies.Commands.CreateTradingViewStrategyById
{
    public class CreateTradingViewStrategyByIdCommand : IRequest<IResult>
    {
        public Guid StrategyId { get; set; }
    }

    public class CreateTradingViewStrategyByIdCommandHandler : IRequestHandler<CreateTradingViewStrategyByIdCommand, IResult>
    {
        private readonly IBaseRepository<Strategy> _strategyRepository;
        private readonly ITradingViewAutomationService _automationService;

        public CreateTradingViewStrategyByIdCommandHandler(
            IBaseRepository<Strategy> strategyRepository,
            ITradingViewAutomationService automationService)
        {
            _strategyRepository = strategyRepository;
            _automationService = automationService;
        }

        public async Task<IResult> Handle(CreateTradingViewStrategyByIdCommand request, CancellationToken cancellationToken)
        {
            var strategy = await _strategyRepository.GetAsync(s => s.Id == request.StrategyId);

            if (strategy == null)
                return new ErrorResult("Strateji bulunamadı.");

            var script = GenerateStrategyScript(strategy);
            var success = await _automationService.CreateStrategyAsync(strategy.StrategyName, strategy.Symbol, script, strategy.WebhookUrl);

            if (success)
                return new SuccessResult("Strateji TradingView'de başarıyla oluşturuldu.");

            return new ErrorResult("TradingView'de strateji oluşturulamadı.");
        }

        private string GenerateStrategyScript(Strategy strategy)
        {
            return $@"
//@version=6
strategy(""{strategy.StrategyName}"", overlay=true)

// Koşullar
longCondition = close > {strategy.EntryPrice}
if (longCondition)
    strategy.entry(""Buy"", strategy.long)

shortCondition = close < {strategy.StopLoss}
if (shortCondition)
    strategy.entry(""Sell"", strategy.short)

// Hedef kar ve zarar sınırları
if (close > {strategy.TakeProfit})
    strategy.close(""Buy"")
if (close < {strategy.StopLoss})
    strategy.close(""Sell"")
";
        }
    }
}
