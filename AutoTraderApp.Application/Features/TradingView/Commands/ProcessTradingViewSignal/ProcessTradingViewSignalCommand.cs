using AutoTraderApp.Application.Features.TradingView.DTOs;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Core.Utilities.Services;
using AutoTraderApp.Domain.Entities;
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
        private readonly IBaseRepository<BrokerAccount> _brokerAccountRepository;
        private readonly IAlpacaService _alpacaService;
        private readonly TradingViewSignalLogService _signalLogService;
        private readonly ITelegramBotService _telegramBotService;

        public ProcessTradingViewSignalCommandHandler(
            IBaseRepository<BrokerAccount> brokerAccountRepository,
            IAlpacaService alpacaService,
           TradingViewSignalLogService signalLogService,
           ITelegramBotService telegramBotService)
        {
            _brokerAccountRepository = brokerAccountRepository;
            _alpacaService = alpacaService;
            _signalLogService = signalLogService;
            _telegramBotService = telegramBotService;
        }

        public async Task<IResult> Handle(ProcessTradingViewSignalCommand request, CancellationToken cancellationToken)
        {
            var signal = request.Signal;

            var brokerAccount = await _brokerAccountRepository.GetAsync(b => b.Id == signal.BrokerAccountId && b.UserId == signal.UserId);
            if (brokerAccount == null)
                return new ErrorResult("Geçerli bir broker hesabı bulunamadı.");

            if (request.Signal.Action == "BUY" || request.Signal.Action == "buy" || request.Signal.Action == "Buy")
            {
                var orderResult = await _alpacaService.PlaceOrderAsync(brokerAccount.Id, new OrderRequest
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
                    await _signalLogService.LogSignalAsync(
           signal.UserId,
           signal.BrokerAccountId,
           signal.Action,
           signal.Symbol,
           signal.Quantity,
           signal.Price,

           "Alım Gerçekleşti",
           $"{signal.Symbol} hissesinden {signal.Quantity} adet sattın alındı ve işlendi."
                    );

                    //telegram notification
                    var telegramUser = await _telegramBotService.GetUserByIdOrPhoneNumberAsync(signal.UserId,null);
                    if (telegramUser?.ChatId != null)
                    {
                        var message = $"{signal.Symbol} Hissesinden {signal.Quantity} Adet Alım İşlemi Gerçekleşti:\n\n" +
                                        $"Hisse: {signal.Symbol}\n" +
                                        $"İşlem: {signal.Action}\n" +
                                        $"Fiyat: {signal.Price}\n" +
                                        $"Miktar: {signal.Quantity}";

                        try
                        {
                            await _telegramBotService.SendMessageAsync(telegramUser.ChatId, message);

                        }
                        catch (Exception ex)
                        {
                            return new ErrorResult($"Telegram Mesaj gönderirken Hata Oluştu: {ex.Message}");
                        }

                    }

                    return new SuccessResult("Alış işlemi başarıyla gerçekleştirildi.");
                }
            }
            else if (request.Signal.Action == "SELL" || request.Signal.Action == "sell" || request.Signal.Action == "Sell")
            {
                var orderResult = await _alpacaService.PlaceOrderAsync(brokerAccount.Id, new OrderRequest
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
                    await _signalLogService.LogSignalAsync(
          signal.UserId,
          signal.BrokerAccountId,
          signal.Action,
          signal.Symbol,
          signal.Quantity,
          signal.Price,
          "Satım Gerçekleşti",
          $"{signal.Symbol} hissesinden {signal.Quantity} adet sattıldı ve işlendi."
      );

                    //telegram notification
                    var telegramUser = await _telegramBotService.GetUserByIdOrPhoneNumberAsync(signal.UserId, null);
                    if (telegramUser?.ChatId != null)
                    {
                        var message = $"{signal.Symbol} Hissesinden {signal.Quantity} Adet Satım İşlemi Gerçekleşti:\n\n" +
                                        $"Hisse: {signal.Symbol}\n" +
                                        $"İşlem: {signal.Action}\n" +
                                        $"Fiyat: {signal.Price}\n" +
                                        $"Miktar: {signal.Quantity}";

                        try
                        {
                            await _telegramBotService.SendMessageAsync(telegramUser.ChatId, message);

                        }
                        catch (Exception ex)
                        {
                            return new ErrorResult($"Telegram Mesaj gönderirken Hata Oluştu: {ex.Message}");
                        }

                    }

                    return new SuccessResult("Satış işlemi başarıyla gerçekleştirildi.");
                }
            }

            return new ErrorResult("İşlem başarısız oldu.");
        }
    }
}