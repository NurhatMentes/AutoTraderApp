using AutoTraderApp.Application.Features.CombinedStocks.Commands;
using AutoTraderApp.Application.Features.NasdaqStocks.Commands;
using AutoTraderApp.Core.Constants;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Core.Utilities.Services;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Infrastructure.Interfaces;
using MediatR;

namespace AutoTraderApp.Application.Features.Strategies.Commands.ApplyStrategyToMultipleStocks
{
    public class ApplyStrategyToMultipleStocksCommand : IRequest<IResult>
    {
        public Guid StrategyId { get; set; }
        public Guid BrokerAccountId { get; set; }
        public Guid UserId { get; set; }
    }

    public class ApplyStrategyToMultipleStocksCommandHandler : IRequestHandler<ApplyStrategyToMultipleStocksCommand, IResult>
    {
        private readonly IBaseRepository<Strategy> _strategyRepository;
        private readonly ITradingViewAutomationService _automationService;
        private readonly IBaseRepository<BrokerAccount> _brokerAccountRepository;
        private readonly IAlpacaService _alpacaService;
        private readonly TradingViewLogService _logService;
        private readonly IBaseRepository<CombinedStock> _combinedStockRepository;
        private readonly IAlphaVantageService _alphaVantageService;
        private readonly IMediator _mediator;
        private readonly IBaseRepository<NasdaqStock> _nasdaqStockRepository;
        private readonly IBaseRepository<CustomStock> _customStockRepository;

        public ApplyStrategyToMultipleStocksCommandHandler(
            IBaseRepository<Strategy> strategyRepository,
            ITradingViewAutomationService automationService,
            IBaseRepository<BrokerAccount> brokerAccountRepository,
            IAlpacaService alpacaService,
            TradingViewLogService logService,
            IBaseRepository<CombinedStock> combinedStockRepository,
            IAlphaVantageService alphaVantageService,
            IMediator mediator,
            IBaseRepository<NasdaqStock> nasdaqStockRepository,
            IBaseRepository<CustomStock> customStockRepository)
        {
            _strategyRepository = strategyRepository;
            _automationService = automationService;
            _brokerAccountRepository = brokerAccountRepository;
            _alpacaService = alpacaService;
            _logService = logService;
            _combinedStockRepository = combinedStockRepository;
            _alphaVantageService = alphaVantageService;
            _mediator = mediator;
            _nasdaqStockRepository = nasdaqStockRepository;
            _customStockRepository = customStockRepository;
        }

        public async Task<IResult> Handle(ApplyStrategyToMultipleStocksCommand request, CancellationToken cancellationToken)
        {
            // Gün sonunda alarmları temizle
            Console.WriteLine("Tüm TradingView alarmları temizleniyor...");
            var deleteAlertsResult = await _automationService.DeleteAllAlertsAsync();
            if (!deleteAlertsResult)
                return new ErrorResult(Messages.Alert.Deleted);

            //**********// Hisse listeleri güncelleniyor && çağırılıyor//**********//

            var updateResult = await _mediator.Send(new UpdateCombinedStockListCommand());
            if (!updateResult)
            {
                return new ErrorResult(Messages.General.Updated);
            }

            var combinedStocks = await _combinedStockRepository.GetAllAsync();
            if (combinedStocks == null || !combinedStocks.Any())
            {
                return new ErrorResult(Messages.General.DataNotFound);
            }

            var updateNasdaq = _mediator.Send(new UpdateNasdaqStocksCommand()).Result;
            if (!updateNasdaq)
            {
                return new ErrorResult(Messages.General.Updated);
            }

            var nasdaqStocks = await _nasdaqStockRepository.GetAllAsync();
            if (nasdaqStocks == null || !nasdaqStocks.Any())
            {
                return new ErrorResult(Messages.General.DataNotFound);
            }

            var customStocks = _customStockRepository.GetAllAsync().Result;
            if (customStocks == null)
            {
                return new ErrorResult(Messages.General.DataNotFound);
            }
            //**********//

            //**********//Kontroller//**********//

            // Strateji bilgisi
            var strategy = await _strategyRepository.GetAsync(s => s.Id == request.StrategyId);
            if (strategy == null)
                return new ErrorResult(Messages.Strategy.NotFound);

            // Broker hesabı doğrulaması
            var brokerAccount = await _brokerAccountRepository.GetAsync(b => b.Id == request.BrokerAccountId && b.UserId == request.UserId);
            if (brokerAccount == null)
                return new ErrorResult(Messages.BrokerAccount.NotFound);

            // Alpaca hesabı doğrulaması
            var account = await _alpacaService.GetAccountInfoAsync(brokerAccount.Id);
            if (account == null)
                return new ErrorResult(Messages.Trading.AccountInfoNotFound);

            decimal accountValue = account.Equity;
            Console.WriteLine($"---------------Hesap değeri: {accountValue}");
            //**********//

            //**********//Alert Oluşturma//**********//

            foreach (var stock in nasdaqStocks)
            {
                int randomTime = new Random().Next(3, 11);
                Console.WriteLine($"---------------Random Time (HANDLE): {randomTime}");

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(randomTime));

                    bool buyAlertSuccess = false;

                    if (!buyAlertSuccess)
                    {
                        buyAlertSuccess = true;
                        buyAlertSuccess = await _automationService.CreateAlertAsync(
                            $"{stock.Symbol}/{strategy.StrategyName}",
                            strategy.WebhookUrl,
                            "{{strategy.order.action}}",
                            stock.Symbol,
                            10,
                            request.BrokerAccountId,
                            request.UserId);

                        if (buyAlertSuccess)
                        {
                            buyAlertSuccess = false;
                            await _logService.LogAsync(request.UserId, request.StrategyId, request.BrokerAccountId, "Buy Alarm Oluşturma", Messages.General.Success, stock.Symbol, "Buy alarmı başarıyla oluşturuldu.");
                        }
                        else
                        {
                            await _logService.LogAsync(request.UserId, request.StrategyId, request.BrokerAccountId, "Buy Alarm Oluşturma", Messages.General.Error, stock.Symbol, "Buy alarmı oluşturulamadı.");
                        }
                    }

                    if (buyAlertSuccess)
                    {
                        await _logService.LogAsync(request.UserId, request.StrategyId, request.BrokerAccountId, "Alarm Oluşturma", Messages.General.Success, stock.Symbol, "--->> Buy ve Sell alarmları başarıyla oluşturuldu.");
                    }
                }
                catch (Exception ex)
                {
                    await _logService.LogAsync(request.UserId, request.StrategyId, request.BrokerAccountId, "Çoklu Strateji Oluşturma", Messages.General.Error, stock.Symbol, $"{Messages.General.SystemError}: {ex.Message}");
                }
            }

            return new SuccessResult(Messages.Strategy.Created);
        }
    }
}