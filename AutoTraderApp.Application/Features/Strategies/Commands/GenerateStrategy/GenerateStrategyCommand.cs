using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using MediatR;

namespace AutoTraderApp.Application.Features.Strategies.Commands
{
    public class GenerateStrategyCommand : IRequest<IResult>
    {
        public string StrategyName { get; set; } = null!;
        public string Symbol { get; set; } = null!;
        public decimal EntryPrice { get; set; }
        public decimal StopLoss { get; set; }
        public decimal TakeProfit { get; set; }
        public string TimeFrame { get; set; } = null!;
    }

    public class GenerateStrategyCommandHandler : IRequestHandler<GenerateStrategyCommand, IResult>
    {
        private readonly IBaseRepository<Domain.Entities.Strategy> _strategyRepository;

        public GenerateStrategyCommandHandler(IBaseRepository<Domain.Entities.Strategy> strategyRepository)
        {
            _strategyRepository = strategyRepository;
        }

        public async Task<IResult> Handle(GenerateStrategyCommand request, CancellationToken cancellationToken)
        {
            var strategy = new Domain.Entities.Strategy
            {
                StrategyName = request.StrategyName,
                Symbol = request.Symbol,
                EntryPrice = request.EntryPrice,
                StopLoss = request.StopLoss,
                TakeProfit = request.TakeProfit,
                TimeFrame = request.TimeFrame,
                CreatedAt = DateTime.UtcNow
            };

            await _strategyRepository.AddAsync(strategy);
            await _strategyRepository.SaveChangesAsync();

            return new SuccessResult("Strateji başarıyla oluşturuldu.");
        }
    }
}
