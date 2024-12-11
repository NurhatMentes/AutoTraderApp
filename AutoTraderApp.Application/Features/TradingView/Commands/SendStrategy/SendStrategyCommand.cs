using AutoMapper;
using AutoTraderApp.Application.Features.TradingView.DTOs;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.ExternalModels.TradingView;
using AutoTraderApp.Infrastructure.Interfaces;
using MediatR;

namespace AutoTraderApp.Application.Features.TradingView.Commands.SendStrategy
{
    public class SendTradingViewStrategyCommand : IRequest<IResult>
    {
        public TradingViewStrategyDto Strategy { get; set; } = null!;
    }

    public class SendTradingViewStrategyCommandHandler : IRequestHandler<SendTradingViewStrategyCommand, IResult>
    {
        private readonly ITradingViewService _tradingViewService;
        private readonly IMapper _mapper;

        public SendTradingViewStrategyCommandHandler(ITradingViewService tradingViewService, IMapper mapper)
        {
            _tradingViewService = tradingViewService;
            _mapper = mapper;
        }

        public async Task<IResult> Handle(SendTradingViewStrategyCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var strategyModel = _mapper.Map<TradingViewStrategy>(request.Strategy);
                var result = await _tradingViewService.SendStrategyAsync(strategyModel);

                if (result)
                    return new SuccessResult("TradingView stratejisi başarıyla gönderildi.");

                return new ErrorResult("TradingView stratejisi gönderilemedi.");
            }
            catch (Exception ex)
            {
                return new ErrorResult($"Hata oluştu: {ex.Message}");
            }
        }
    }
}
