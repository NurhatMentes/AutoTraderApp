using AutoTraderApp.Core.Utilities.Results;
using MediatR;

namespace AutoTraderApp.Application.Features.Strategies.Commands
{
    public class GenerateStrategyCommand : IRequest<IDataResult<string>>
    {
        public string ShortPeriod { get; set; }
        public string LongPeriod { get; set; }
        public string StrategyName { get; set; }
    }

    public class GenerateStrategyCommandHandler : IRequestHandler<GenerateStrategyCommand, IDataResult<string>>
    {
        public async Task<IDataResult<string>> Handle(GenerateStrategyCommand request, CancellationToken cancellationToken)
        {
            var pineScript = $@"
            //@version=5
            strategy('{request.StrategyName}', overlay=true)
            shortMA = ta.sma(close, {request.ShortPeriod})
            longMA = ta.sma(close, {request.LongPeriod})

            if (ta.crossover(shortMA, longMA))
                strategy.entry('Buy', strategy.long)

            if (ta.crossunder(shortMA, longMA))
                strategy.close('Buy')
        ";

            return new SuccessDataResult<string>("Strateji başarıyla oluşturuldu.", data: pineScript);
        }
    }
}
