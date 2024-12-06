using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Infrastructure.Interfaces;
using MediatR;

namespace AutoTraderApp.Application.Features.Strategies.Commands.UploadStrategy
{
    public class UploadStrategyCommand : IRequest<IResult>
    {
        public string StrategyName { get; set; }
        public string PineScriptCode { get; set; }
    }

    public class UploadStrategyCommandHandler : IRequestHandler<UploadStrategyCommand, IResult>
    {
        private readonly ITradingViewService _tradingViewService;

        public UploadStrategyCommandHandler(ITradingViewService tradingViewService)
        {
            _tradingViewService = tradingViewService;
        }

        public async Task<IResult> Handle(UploadStrategyCommand request, CancellationToken cancellationToken)
        {
            var result = await _tradingViewService.UploadStrategyAsync(request.StrategyName, request.PineScriptCode);

            if (result)
                return new SuccessResult("Strateji başarıyla yüklendi.");

            return new ErrorResult("Strateji yükleme başarısız oldu.");
        }
    }
}
