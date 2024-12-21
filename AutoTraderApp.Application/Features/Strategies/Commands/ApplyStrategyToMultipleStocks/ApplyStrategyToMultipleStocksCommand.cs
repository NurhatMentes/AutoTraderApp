using AutoTraderApp.Core.Utilities.Calculators;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Core.Utilities.Services;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Infrastructure.Interfaces;
using MediatR;
using System.Globalization;

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

        public ApplyStrategyToMultipleStocksCommandHandler(
            IBaseRepository<Strategy> strategyRepository,
            ITradingViewAutomationService automationService,
            IBaseRepository<BrokerAccount> brokerAccountRepository,
            IAlpacaService alpacaService,
            TradingViewLogService logService,
            IBaseRepository<CombinedStock> combinedStockRepository)
        {
            _strategyRepository = strategyRepository;
            _automationService = automationService;
            _brokerAccountRepository = brokerAccountRepository;
            _alpacaService = alpacaService;
            _logService = logService;
            _combinedStockRepository = combinedStockRepository;
        }

        public async Task<IResult> Handle(ApplyStrategyToMultipleStocksCommand request, CancellationToken cancellationToken)
        {
            var combinedStocks = await _combinedStockRepository.GetAllAsync();
            if (combinedStocks == null || !combinedStocks.Any())
            {
                return new ErrorResult("Birleşik hisse listesi bulunamadı.");
            }

            var strategy = await _strategyRepository.GetAsync(s => s.Id == request.StrategyId);
            if (strategy == null)
                return new ErrorResult("Strateji bulunamadı.");

            var brokerAccount = await _brokerAccountRepository.GetAsync(b => b.Id == request.BrokerAccountId && b.UserId == request.UserId);
            if (brokerAccount == null)
                return new ErrorResult("Geçerli bir broker hesabı bulunamadı.");

            var account = await _alpacaService.GetAccountInfoAsync(brokerAccount.Id);
            if (account == null)
                return new ErrorResult("Kullanıcı hesabı bilgileri alınamadı.");

            decimal portfolioValue = account.Cash;
            decimal riskPercentage = 0.02m;

            var symbols = combinedStocks.Select(cs => cs.Symbol).ToList();

            foreach (var symbol in symbols)
            {
                try
                {
                    int quantity = QuantityCalculator.CalculateQuantity(portfolioValue, riskPercentage, strategy.EntryPrice, strategy.StopLoss);
                    var script = GenerateStrategyScript(strategy, quantity, symbol);

                    var strategySuccess = await _automationService.CreateStrategyAsync(strategy.StrategyName + " " + symbol, symbol, script, strategy.WebhookUrl, request.UserId);
                    if (!strategySuccess)
                    {
                        await _logService.LogAsync(request.UserId, request.StrategyId, request.BrokerAccountId, "Strateji Oluşturma", "Hata", symbol, "Strateji oluşturulamadı.");
                        continue;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(5));

                    var alertSuccess = await _automationService.CreateAlertAsync(
                        strategy.StrategyName + " " + symbol,
                        strategy.WebhookUrl,
                        "buy",
                        symbol,
                        quantity,
                        strategy.EntryPrice,
                        request.BrokerAccountId,
                        request.UserId);

                    if (!alertSuccess)
                    {
                        await _logService.LogAsync(request.UserId, request.StrategyId, request.BrokerAccountId, "Çoklu Alarm Oluşturma", "Hata", symbol, "Alarm oluşturulamadı.");
                        continue;
                    }

                    await _logService.LogAsync(request.UserId, request.StrategyId, request.BrokerAccountId, "Çoklu Strateji Oluşturma", "Hata",symbol,"Strateji oluşturulamadı.");
                }
                catch (Exception ex)
                {
                    await _logService.LogAsync(request.UserId, request.StrategyId, request.BrokerAccountId, "Çoklu Strateji Oluşturma", "Hata", symbol, $"Hata: {ex.Message}");
                }
            }

            return new SuccessResult("Strateji belirtilen hisselere başarıyla uygulandı.");
        }

        private string GenerateStrategyScript(Strategy strategy, int quantity, string symbol)
        {
            return $@"
//@version=6
strategy(""{strategy.StrategyName}"", overlay=true)

// Koşullar
longCondition = close > {strategy.EntryPrice.ToString(CultureInfo.InvariantCulture)}
if (longCondition)
    strategy.entry(""Buy"", strategy.long, qty={quantity})

shortCondition = close < {strategy.StopLoss.ToString(CultureInfo.InvariantCulture)}
if (shortCondition)
    strategy.entry(""Sell"", strategy.short, qty={quantity})

// Hedef kar ve zarar sınırları
if (close > {strategy.TakeProfit.ToString(CultureInfo.InvariantCulture)})
    strategy.close(""Buy"")
if (close < {strategy.StopLoss.ToString(CultureInfo.InvariantCulture)})
    strategy.close(""Sell"")
";
        }
    }
}
