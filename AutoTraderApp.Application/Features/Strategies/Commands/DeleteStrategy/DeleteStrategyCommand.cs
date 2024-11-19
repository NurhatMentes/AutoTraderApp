using AutoMapper;
using AutoTraderApp.Application.Contracts.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutoTraderApp.Application.Features.Strategies.Commands.DeleteStrategy
{
    public class DeleteStrategyCommand : IRequest<IResult>
    {
        public Guid Id { get; set; }

        public class DeleteStrategyCommandHandler : IRequestHandler<DeleteStrategyCommand, IResult>
        {
            private readonly IBaseRepository<Strategy> _strategyRepository;
            private readonly IBaseRepository<TradingRule> _tradingRuleRepository;
            private readonly ILogger<DeleteStrategyCommandHandler> _logger;
            private readonly IMapper _mapper;

            public DeleteStrategyCommandHandler(
                IBaseRepository<Strategy> strategyRepository,
                IBaseRepository<TradingRule> tradingRuleRepository,
                ILogger<DeleteStrategyCommandHandler> logger,
                IMapper mapper)
            {
                _strategyRepository = strategyRepository;
                _tradingRuleRepository = tradingRuleRepository;
                _logger = logger;
                _mapper = mapper;
            }

            public async Task<IResult> Handle(DeleteStrategyCommand request, CancellationToken cancellationToken)
            {
                _logger.LogInformation("DeleteStrategyCommand handling started for StrategyId: {StrategyId}", request.Id);

                var strategy = await _strategyRepository.GetAsync(s => s.Id == request.Id);
                if (strategy == null)
                {
                    _logger.LogWarning("Strategy not found for deletion. StrategyId: {StrategyId}", request.Id);
                    return new ErrorResult("Silinecek strateji bulunamadı.");
                }

                try
                {
                    // Önce ilişkili alım-satım kurallarını sil
                    var tradingRules = await _tradingRuleRepository.GetListWithStringIncludeAsync(
                        r => r.StrategyId == request.Id);

                    foreach (var rule in tradingRules)
                    {
                        await _tradingRuleRepository.DeleteAsync(rule);
                    }

                    // Sonra stratejiyi sil
                    await _strategyRepository.DeleteAsync(strategy);

                    _logger.LogInformation("Strategy and related trading rules successfully deleted. StrategyId: {StrategyId}", request.Id);
                    return new SuccessResult("Alım-satım stratejisi ve ilişkili kuralları başarıyla silindi.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while deleting strategy. StrategyId: {StrategyId}", request.Id);
                    return new ErrorResult("Strateji silinirken bir hata oluştu.");
                }
            }
        }
    }

}
