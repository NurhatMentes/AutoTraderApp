using AutoTraderApp.Application.Features.TradingView.DTOs;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.ExternalModels.Alpaca.Models;
using AutoTraderApp.Infrastructure.Interfaces;
using MediatR;

namespace AutoTraderApp.Application.Features.TradingView.Commands.ProcessTradingViewSignal
{
    public class ProcessTradingViewSignalCommand : IRequest<IResult>
    {
        public TradingViewSignalDto Signal { get; set; } = null!;
    }

    public class ProcessTradingViewSignalCommandHandler : IRequestHandler<ProcessTradingViewSignalCommand, IResult>
    {
        private readonly IAlpacaService _alpacaService;

        public ProcessTradingViewSignalCommandHandler(IAlpacaService alpacaService)
        {
            _alpacaService = alpacaService;
        }

        public async Task<IResult> Handle(ProcessTradingViewSignalCommand request, CancellationToken cancellationToken)
        {

            if (request.Signal.Action == "BUY" || request.Signal.Action == "buy" || request.Signal.Action == "Buy")
            {
                var orderResult = await _alpacaService.PlaceOrderAsync(new OrderRequest
                {
                    Symbol = request.Signal.Symbol,
                    Qty = request.Signal.Quantity,
                    Side = "buy",
                    Type = "market",
                    TimeInForce = "gtc"
                });

                if (orderResult.Status == "accepted" ||
                    orderResult.Status == "new" ||
                    orderResult.Status == "pending_new" ||
                    orderResult.Status == "partially_filled" ||
                    orderResult.Status == "filled")
                {
                    return new SuccessResult("Alış işlemi başarıyla gerçekleştirildi.");
                }
            }
            else if (request.Signal.Action == "SELL" || request.Signal.Action == "sell" || request.Signal.Action == "Sell")
            {
                var orderResult = await _alpacaService.PlaceOrderAsync(new OrderRequest
                {
                    Symbol = request.Signal.Symbol,
                    Qty = request.Signal.Quantity,
                    Side = "sell",
                    Type = "market",
                    TimeInForce = "gtc"
                });

                if (orderResult.Status == "accepted" ||
                    orderResult.Status == "new" ||
                    orderResult.Status == "pending_new" ||
                    orderResult.Status == "partially_filled" ||
                    orderResult.Status == "filled")
                {
                    return new SuccessResult("Satış işlemi başarıyla gerçekleştirildi.");
                }
            }

            return new ErrorResult("İşlem başarısız oldu.");
        }
    }
}