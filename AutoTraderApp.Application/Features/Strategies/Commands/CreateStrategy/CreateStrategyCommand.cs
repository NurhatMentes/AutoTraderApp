using AutoTraderApp.Application.Features.Strategies.DTOs;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using MediatR;

namespace AutoTraderApp.Application.Features.Strategies.Commands.CreateStrategy
{
    public class CreateStrategyCommand : IRequest<IResult>
    {
        public CreateStrategyRequest createStrategyDto { get; set; }

        public CreateStrategyCommand(CreateStrategyRequest createStrategyDto)
        {
            this.createStrategyDto = createStrategyDto;
        }

        public class CreateStrategyCommandHandler : IRequestHandler<CreateStrategyCommand, IResult>
        {
            private readonly IBaseRepository<Strategy> _strategyRepository;

            public CreateStrategyCommandHandler(IBaseRepository<Strategy> strategyRepository)
            {
                _strategyRepository = strategyRepository;
            }

            public async Task<IResult> Handle(CreateStrategyCommand request, CancellationToken cancellationToken)
            {
                try
                {
                    // Stratejiyi oluştur
                    var strategy = new Strategy
                    {
                        StrategyName = request.createStrategyDto.StrategyName,
                        Symbol = request.createStrategyDto.Symbol,
                        EntryPrice = request.createStrategyDto.EntryPrice,
                        StopLoss = request.createStrategyDto.StopLoss,
                        TakeProfit = request.createStrategyDto.TakeProfit,
                        TimeFrame = request.createStrategyDto.TimeFrame,
                        WebhookUrl = request.createStrategyDto.WebhookUrl,
                        AtrLength = request.createStrategyDto.AtrLength,
                        BollingerLength = request.createStrategyDto.BollingerLength,
                        BollingerMultiplier = request.createStrategyDto.BollingerMultiplier,
                        DmiLength = request.createStrategyDto.DmiLength,
                        AdxSmoothing = request.createStrategyDto.AdxSmoothing,
                        AdxThreshold = request.createStrategyDto.AdxThreshold,
                        RsiLength = request.createStrategyDto.RsiLength,
                        RsiUpper = request.createStrategyDto.RsiUpper,
                        RsiLower = request.createStrategyDto.RsiLower,
                        StochRsiLength = request.createStrategyDto.StochRsiLength,
                        StochRsiUpper = request.createStrategyDto.StochRsiUpper,
                        StochRsiLower = request.createStrategyDto.StochRsiLower
                    };

                    // Veritabanına ekle
                    await _strategyRepository.AddAsync(strategy);

                    return new SuccessResult("Strateji başarıyla oluşturuldu.");
                }
                catch (Exception ex)
                {
                    return new ErrorResult($"Strateji oluşturulurken hata oluştu: {ex.Message}");
                }
            }
        }
    }
}