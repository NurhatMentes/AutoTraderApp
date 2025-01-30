using AutoTraderApp.Application.Features.CombinedStocks.Commands;
using AutoTraderApp.Application.Features.Strategies.Helpers;
using AutoTraderApp.Core.Constants;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Core.Utilities.Services;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Infrastructure.Interfaces;
using MediatR;

namespace AutoTraderApp.Application.Features.Strategies.Commands.ApplyStrategyToMultipleStocksSync
{
    public class ApplyStrategyToMultipleStocksSyncCommand : IRequest<IResult>
    {
        public Guid StrategyId { get; set; }
        public Guid BrokerAccountId { get; set; }
        public Guid UserId { get; set; }
        public bool isMarginTrade { get; set; }
    }

    public class ApplyStrategyToMultipleStocksSyncCommandHandler : IRequestHandler<ApplyStrategyToMultipleStocksSyncCommand, IResult>
    {
        private readonly IBaseRepository<Strategy> _strategyRepository;
        private readonly ITradingViewAutomationService _automationService;
        private readonly IBaseRepository<BrokerAccount> _brokerAccountRepository;
        private readonly IAlpacaService _alpacaService;
        private readonly TradingViewLogService _logService;
        private readonly IBaseRepository<CombinedStock> _combinedStockRepository;
        private readonly IAlphaVantageService _alphaVantageService;
        private readonly IMediator _mediator;
        private readonly ITradingViewSeleniumService _tradingViewSeleniumService;
        private readonly IBaseRepository<NasdaqStock> _nasdaqStockRepository;
        private readonly IBaseRepository<CustomStock> _customStockRepository;

        public ApplyStrategyToMultipleStocksSyncCommandHandler(
            IBaseRepository<Strategy> strategyRepository,
            ITradingViewAutomationService automationService,
            IBaseRepository<BrokerAccount> brokerAccountRepository,
            IAlpacaService alpacaService,
            TradingViewLogService logService,
            IBaseRepository<CombinedStock> combinedStockRepository,
            IAlphaVantageService alphaVantageService,
            IMediator mediator,
            ITradingViewSeleniumService tradingViewSeleniumService,
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
            _tradingViewSeleniumService = tradingViewSeleniumService;
            _nasdaqStockRepository = nasdaqStockRepository;
            _customStockRepository = customStockRepository;
        }

        public Task<IResult> Handle(ApplyStrategyToMultipleStocksSyncCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var result = HandleSynchronously(request);
                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                return Task.FromResult<IResult>(new ErrorResult($"{Messages.General.SystemError}: {ex.Message}"));
            }
        }

        private IResult HandleSynchronously(ApplyStrategyToMultipleStocksSyncCommand request)
        {
            // Gün sonunda alarmları temizle
            Console.WriteLine("Tüm TradingView alarmları temizleniyor...");
            var deleteAlertsResult = _automationService.DeleteAllAlertsAsync().Result;
            if (!deleteAlertsResult)
                return new ErrorResult(Messages.Alert.Deleted);

            //**********//Hisse listeleri güncelleniyor && çağırılıyor//**********//

            // UpdateCombinedStockListCommand çalıştırılıyor
            var updateResult = _mediator.Send(new UpdateCombinedStockListCommand()).Result;
            if (!updateResult)
            {
                return new ErrorResult(Messages.General.Updated);
            }

            var combinedStocks = _combinedStockRepository.GetAllAsync().Result;
            if (combinedStocks == null || !combinedStocks.Any())
            {
                return new ErrorResult(Messages.General.DataNotFound);
            }

            var nasdaqStocks = _alphaVantageService.GetNasdaqListingsAsync(500).Result;
            if (nasdaqStocks == null)
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
            var strategy = _strategyRepository.GetAsync(s => s.Id == request.StrategyId).Result;
            if (strategy == null)
                return new ErrorResult(Messages.Strategy.NotFound);

            // Broker hesabı doğrulaması
            var brokerAccount = _brokerAccountRepository.GetAsync(b => b.Id == request.BrokerAccountId && b.UserId == request.UserId).Result;
            if (brokerAccount == null)
                return new ErrorResult(Messages.BrokerAccount.NotFound);

            // Alpaca hesabı doğrulaması
            var account = _alpacaService.GetAccountInfoAsync(brokerAccount.Id).Result;
            if (account == null)
                return new ErrorResult(Messages.Trading.AccountInfoNotFound);
            //**********//

            decimal accountValue = account.Equity;
            Console.WriteLine($"---------------Hesap değeri: {accountValue}");

            //**********//Alert Oluşturma//**********//

            foreach (var stock in customStocks)
            {
                Console.WriteLine($"---------------Seçilen hisse: {stock.Symbol}");

                try
                {
                    var symbol = stock.Symbol;
                   
                    var alertName = $"{brokerAccount.BrokerType}//{strategy.StrategyName}";

                    Console.WriteLine($"---------------BUY Alert Creating for: {stock.Symbol}");
                    var buyAlertSuccess = _tradingViewSeleniumService.CreateAlertSync(
                        alertName,
                        strategy.WebhookUrl,
                        "{{strategy.order.action}}",
                        stock.Symbol,
                        10,
                        request.BrokerAccountId,
                        request.UserId,
                        request.isMarginTrade
                        );

                    if (buyAlertSuccess)
                    {
                        Console.WriteLine($"Buy Alert Successfully Created for {stock.Symbol}");
                        _logService.LogAsync(request.UserId, request.StrategyId, request.BrokerAccountId, "Alarm Oluşturma", Messages.General.Success, stock.Symbol, "Buy ve Sell alarmları başarıyla oluşturuldu.").Wait();
                    }
                    else
                    {
                        Console.WriteLine($"Failed to Create Buy Alert for {stock.Symbol}");
                        _logService.LogAsync(request.UserId, request.StrategyId, request.BrokerAccountId, "Alarm Oluşturma", Messages.General.Error, stock.Symbol, "Buy alarmı oluşturulamadı.").Wait();
                    }
                }
                catch (Exception ex)
                {
                    _logService.LogAsync(request.UserId, request.StrategyId, request.BrokerAccountId, "Çoklu Strateji Oluşturma", Messages.General.Error, stock.Symbol, $"{Messages.General.SystemError}: {ex.Message}").Wait();
                }
            }

            return new SuccessResult(Messages.Strategy.Created);
        }
    }
}