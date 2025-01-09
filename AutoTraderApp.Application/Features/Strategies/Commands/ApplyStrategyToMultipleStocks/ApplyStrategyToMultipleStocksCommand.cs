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

        public ApplyStrategyToMultipleStocksCommandHandler(
            IBaseRepository<Strategy> strategyRepository,
            ITradingViewAutomationService automationService,
            IBaseRepository<BrokerAccount> brokerAccountRepository,
            IAlpacaService alpacaService,
            TradingViewLogService logService,
            IBaseRepository<CombinedStock> combinedStockRepository,
            IAlphaVantageService alphaVantageService,
            IMediator mediator)
        {
            _strategyRepository = strategyRepository;
            _automationService = automationService;
            _brokerAccountRepository = brokerAccountRepository;
            _alpacaService = alpacaService;
            _logService = logService;
            _combinedStockRepository = combinedStockRepository;
            _alphaVantageService = alphaVantageService;
            _mediator = mediator;
        }

        public async Task<IResult> Handle(ApplyStrategyToMultipleStocksCommand request, CancellationToken cancellationToken)
        {
            // Gün sonunda alarmları temizle
            Console.WriteLine("Tüm TradingView alarmları temizleniyor...");
            var deleteAlertsResult = await _automationService.DeleteAllAlertsAsync();
            if (!deleteAlertsResult)
                return new ErrorResult("TradingView alarmları temizlenemedi.");

            // UpdateCombinedStockListCommand çalıştırılıyor
            var updateResult = await _mediator.Send(new UpdateCombinedStockListCommand());
            if (!updateResult)
            {
                return new ErrorResult($"Birleşik hisse güncellenemedi");
            }

            var combinedStocks = await _combinedStockRepository.GetAllAsync();
            if (combinedStocks == null || !combinedStocks.Any())
            {
                return new ErrorResult("Birleşik hisse listesi bulunamadı.");
            }

            // Strateji bilgisi
            var strategy = await _strategyRepository.GetAsync(s => s.Id == request.StrategyId);
            if (strategy == null)
                return new ErrorResult("Strateji bulunamadı.");

            // Broker hesabı doğrulaması
            var brokerAccount = await _brokerAccountRepository.GetAsync(b => b.Id == request.BrokerAccountId && b.UserId == request.UserId);
            if (brokerAccount == null)
                return new ErrorResult("Geçerli bir broker hesabı bulunamadı.");

            // Alpaca hesabı doğrulaması
            var account = await _alpacaService.GetAccountInfoAsync(brokerAccount.Id);
            if (account == null)
                return new ErrorResult("Kullanıcı hesabı bilgileri alınamadı.");

            decimal accountValue = account.Equity;
            Console.WriteLine($"---------------Hesap değeri: {accountValue}");



            decimal riskPercentage = StockSelectionHelper.CalculateRiskPercentage(accountValue);
            var selectedStocks = StockSelectionHelper.SelectStocks(combinedStocks, accountValue);
            Console.WriteLine($"---------------Risk yüzdesi: {riskPercentage}");

            foreach (var selectedStock in selectedStocks)
            {
                Console.WriteLine($"---------------Seçilen hisseler: {selectedStock.Symbol} --- ");
            }

            foreach (var stock in selectedStocks)
            {
                int randomTime = new Random().Next(3, 11);
                Console.WriteLine($"---------------Random Time (HANDLE): {randomTime}");

                try
                {
                    int quantity = QuantityCalculator.CalculateQuantity(accountValue, riskPercentage, stock.Price ?? 0, stock.Price.Value * 0.95m);
                    string script = StrategyScriptGenerator.GenerateScript(strategy, quantity, stock.Symbol);

                    await Task.Delay(TimeSpan.FromSeconds(randomTime));

                    bool buyAlertSuccess = false;
                    bool sellAlertSuccess = false;

                    decimal price = Convert.ToDecimal(stock.Price.Value);
                    if (!buyAlertSuccess)
                    {
                        buyAlertSuccess = await _automationService.CreateAlertAsync(
                            $"{stock.Symbol}/Buy/{strategy.StrategyName}",
                            strategy.WebhookUrl,
                            "buy",
                            stock.Symbol,
                            quantity,
                            request.BrokerAccountId,
                            request.UserId);

                        if (buyAlertSuccess)
                        {
                            await _logService.LogAsync(request.UserId, request.StrategyId, request.BrokerAccountId, "Buy Alarm Oluşturma", "Başarılı", stock.Symbol, "Buy alarmı başarıyla oluşturuldu.");
                        }
                        else
                        {
                            await _logService.LogAsync(request.UserId, request.StrategyId, request.BrokerAccountId, "Buy Alarm Oluşturma", "Hata", stock.Symbol, "Buy alarmı oluşturulamadı.");
                        }
                    }

                    await Task.Delay(TimeSpan.FromSeconds(3));

                    if (!sellAlertSuccess)
                    {
                        sellAlertSuccess = await _automationService.CreateAlertAsync(
                            $"{stock.Symbol}/Sell/{strategy.StrategyName}",
                            strategy.WebhookUrl,
                            "sell",
                            stock.Symbol,
                            quantity,
                            request.BrokerAccountId,
                            request.UserId);

                        if (sellAlertSuccess)
                        {
                            await _logService.LogAsync(request.UserId, request.StrategyId, request.BrokerAccountId, "Sell Alarm Oluşturma", "Başarılı", stock.Symbol, "Sell alarmı başarıyla oluşturuldu.");
                        }
                        else
                        {
                            await _logService.LogAsync(request.UserId, request.StrategyId, request.BrokerAccountId, "Sell Alarm Oluşturma", "Hata", stock.Symbol, "Sell alarmı oluşturulamadı.");
                        }
                    }

                    if (buyAlertSuccess && sellAlertSuccess)
                    {
                        await _logService.LogAsync(request.UserId, request.StrategyId, request.BrokerAccountId, "Alarm Oluşturma", "Başarılı", stock.Symbol, "--->> Buy ve Sell alarmları başarıyla oluşturuldu.");
                    }
                }
                catch (Exception ex)
                {
                    await _logService.LogAsync(request.UserId, request.StrategyId, request.BrokerAccountId, "Çoklu Strateji Oluşturma", "Hata", stock.Symbol, $"Hata: {ex.Message}");
                }
            }



            return new SuccessResult("Strateji belirtilen hisselere başarıyla uygulandı.");
        }

    }
}