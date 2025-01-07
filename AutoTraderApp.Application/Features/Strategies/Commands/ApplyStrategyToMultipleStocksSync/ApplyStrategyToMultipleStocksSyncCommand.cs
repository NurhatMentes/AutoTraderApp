using AutoTraderApp.Application.Features.CombinedStocks.Commands;
using AutoTraderApp.Application.Features.Strategies.Helpers;
using AutoTraderApp.Core.Utilities.Calculators;
using AutoTraderApp.Core.Utilities.Generator;
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
    }

    public class ApplyStrategyToMultipleStocksCommandHandler : IRequestHandler<ApplyStrategyToMultipleStocksSyncCommand, IResult>
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

        public ApplyStrategyToMultipleStocksCommandHandler(
            IBaseRepository<Strategy> strategyRepository,
            ITradingViewAutomationService automationService,
            IBaseRepository<BrokerAccount> brokerAccountRepository,
            IAlpacaService alpacaService,
            TradingViewLogService logService,
            IBaseRepository<CombinedStock> combinedStockRepository,
            IAlphaVantageService alphaVantageService,
            IMediator mediator,
            ITradingViewSeleniumService tradingViewSeleniumService)
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
                return Task.FromResult<IResult>(new ErrorResult($"Bir hata oluştu: {ex.Message}"));
            }
        }

        private IResult HandleSynchronously(ApplyStrategyToMultipleStocksSyncCommand request)
        {
            // Gün sonunda alarmları temizle
            Console.WriteLine("Tüm TradingView alarmları temizleniyor...");
            var deleteAlertsResult = _automationService.DeleteAllAlertsAsync().Result;
            if (!deleteAlertsResult)
                return new ErrorResult("TradingView alarmları temizlenemedi.");

            // UpdateCombinedStockListCommand çalıştırılıyor
            var updateResult = _mediator.Send(new UpdateCombinedStockListCommand()).Result;
            if (!updateResult)
            {
                return new ErrorResult($"Birleşik hisse güncellenemedi");
            }

            var combinedStocks = _combinedStockRepository.GetAllAsync().Result;
            if (combinedStocks == null || !combinedStocks.Any())
            {
                return new ErrorResult("Birleşik hisse listesi bulunamadı.");
            }

            // Strateji bilgisi
            var strategy = _strategyRepository.GetAsync(s => s.Id == request.StrategyId).Result;
            if (strategy == null)
                return new ErrorResult("Strateji bulunamadı.");

            // Broker hesabı doğrulaması
            var brokerAccount = _brokerAccountRepository.GetAsync(b => b.Id == request.BrokerAccountId && b.UserId == request.UserId).Result;
            if (brokerAccount == null)
                return new ErrorResult("Geçerli bir broker hesabı bulunamadı.");

            // Alpaca hesabı doğrulaması
            var account = _alpacaService.GetAccountInfoAsync(brokerAccount.Id).Result;
            if (account == null)
                return new ErrorResult("Kullanıcı hesabı bilgileri alınamadı.");

            decimal accountValue = account.Equity;
            Console.WriteLine($"---------------Hesap değeri: {accountValue}");

            decimal riskPercentage = StockSelectionHelper.CalculateRiskPercentage(accountValue);
            var selectedStocks = StockSelectionHelper.SelectStocks(combinedStocks, accountValue);
            Console.WriteLine($"---------------Risk yüzdesi: {riskPercentage}");

            foreach (var stock in selectedStocks)
            {
                Console.WriteLine($"---------------Seçilen hisse: {stock.Symbol}");

                try
                {
                    int quantity = QuantityCalculator.CalculateQuantity(accountValue, riskPercentage, stock.Price ?? 0, stock.Price.Value * 0.95m);
                    string script = StrategyScriptGenerator.GenerateScript(strategy, quantity, stock.Symbol);

                    var symbol = stock.Symbol;

                    Console.WriteLine($"---------------BUY Alert Creating for: {stock.Symbol}");
                    var buyAlertSuccess = _tradingViewSeleniumService.CreateAlertSync(
                        $"{symbol}/Buy/{strategy.StrategyName}",
                        strategy.WebhookUrl,
                        "buy",
                        stock.Symbol,
                        quantity,
                        request.BrokerAccountId,
                        request.UserId);

                    if (buyAlertSuccess)
                    {
                        Console.WriteLine($"Buy Alert Successfully Created for {stock.Symbol}");
                    }
                    else
                    {
                        Console.WriteLine($"Failed to Create Buy Alert for {stock.Symbol}");
                    }


                    Thread.Sleep(TimeSpan.FromSeconds(2));
                    Console.WriteLine($"---------------SELL Alert Creating for: {stock.Symbol}");
                    var sellAlertSuccess = _tradingViewSeleniumService.CreateAlertSync(
                        $"{symbol}/Sell/{strategy.StrategyName}",
                        strategy.WebhookUrl,
                        "sell",
                        stock.Symbol,
                        quantity,
                        request.BrokerAccountId,
                        request.UserId);

                    if (sellAlertSuccess)
                    {
                        _logService.LogAsync(request.UserId, request.StrategyId, request.BrokerAccountId, "Sell Alarm Oluşturma", "Başarılı", stock.Symbol, "Sell alarmı başarıyla oluşturuldu.").Wait();
                    }
                    else
                    {
                        _logService.LogAsync(request.UserId, request.StrategyId, request.BrokerAccountId, "Sell Alarm Oluşturma", "Hata", stock.Symbol, "Sell alarmı oluşturulamadı.").Wait();
                    }

                    if (buyAlertSuccess && sellAlertSuccess)
                    {
                        _logService.LogAsync(request.UserId, request.StrategyId, request.BrokerAccountId, "Alarm Oluşturma", "Başarılı", stock.Symbol, "Buy ve Sell alarmları başarıyla oluşturuldu.").Wait();
                    }
                }
                catch (Exception ex)
                {
                    _logService.LogAsync(request.UserId, request.StrategyId, request.BrokerAccountId, "Çoklu Strateji Oluşturma", "Hata", stock.Symbol, $"Hata: {ex.Message}").Wait();
                }
            }

            return new SuccessResult("Strateji belirtilen hisselere başarıyla uygulandı.");
        }


    }
}