using AutoTraderApp.Core.Utilities.Calculators;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Core.Utilities.Services;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Infrastructure.Interfaces;
using MediatR;
using System.Globalization;

namespace AutoTraderApp.Application.Features.Strategies.Commands.CreateTradingViewStrategyById
{
    public class CreateTradingViewStrategyByIdCommand : IRequest<IResult>
    {
        public Guid StrategyId { get; set; }
        public Guid BrokerAccountId { get; set; }
        public Guid UserId { get; set; }
    }

    public class CreateTradingViewStrategyByIdCommandHandler : IRequestHandler<CreateTradingViewStrategyByIdCommand, IResult>
    {
        private readonly IBaseRepository<Strategy> _strategyRepository;
        private readonly ITradingViewAutomationService _automationService;
        private readonly IBaseRepository<BrokerAccount> _brokerAccountRepository;
        private readonly IAlpacaService _alpacaService;
        private readonly TradingViewLogService _logService;

        public CreateTradingViewStrategyByIdCommandHandler(
            IBaseRepository<Strategy> strategyRepository,
            ITradingViewAutomationService automationService,
            IBaseRepository<BrokerAccount> brokerAccountRepository,
            IAlpacaService alpacaService,
            TradingViewLogService logService
        )
        {
            _strategyRepository = strategyRepository;
            _automationService = automationService;
            _brokerAccountRepository = brokerAccountRepository;
            _alpacaService = alpacaService;
            _logService = logService;
        }

        public async Task<IResult> Handle(CreateTradingViewStrategyByIdCommand request, CancellationToken cancellationToken)
        {
            var strategy = await _strategyRepository.GetAsync(s => s.Id == request.StrategyId);
            if (strategy == null)
            {
                await _logService.LogAsync(request.UserId, request.StrategyId, request.BrokerAccountId, "Strateji Kontrolü", "Hata", "-", "Strateji bulunamadı.");
                return new ErrorResult("Strateji bulunamadı.");
            }



            var brokerAccount = await _brokerAccountRepository.GetAsync(b => b.Id == request.BrokerAccountId && b.UserId == request.UserId);
            if (brokerAccount == null)
            {
                await _logService.LogAsync(request.UserId, request.StrategyId, request.BrokerAccountId, "Broker Hesabı Kontrolü", "Hata", "-", "Geçerli bir broker hesabı bulunamadı.");
                return new ErrorResult("Geçerli bir broker hesabı bulunamadı.");
            }



            var account = await _alpacaService.GetAccountInfoAsync(brokerAccount.Id);
            if (account == null)
            {
                await _logService.LogAsync(request.UserId, request.StrategyId, request.BrokerAccountId, "Kullanıcı Hesabı Kontrolü", "Hata", "-", "Kullanıcı hesabı bilgileri alınamadı.");
                return new ErrorResult("Kullanıcı hesabı bilgileri alınamadı.");
            }



            decimal portfolioValue = account.Cash; 
            decimal riskPercentage = 0.02m; 

            int quantity;
            try
            {
                quantity = QuantityCalculator.CalculateQuantity(portfolioValue, riskPercentage, strategy.EntryPrice, strategy.StopLoss);
            }
            catch (Exception ex)
            {
                return new ErrorResult($"Miktar hesaplama hatası: {ex.Message}");
            }



            var script = GenerateStrategyScript(strategy, quantity);

            var strategySuccess = await _automationService.CreateStrategyAsync(strategy.StrategyName, strategy.Symbol, script, strategy.WebhookUrl, request.UserId);
            if (!strategySuccess)
            {
                await _logService.LogAsync(request.UserId, request.StrategyId, request.BrokerAccountId, "Strateji Oluşturma", "Hata", strategy.Symbol, "Strateji oluşturulamadı.");
                return new ErrorResult("TradingView'de strateji oluşturulamadı.");
            }
            await Task.Delay(TimeSpan.FromSeconds(5));



            var alertSuccess = await _automationService.CreateAlertAsync(
                strategy.StrategyName,
                strategy.WebhookUrl,
                "buy",
                strategy.Symbol,
                quantity,
                strategy.EntryPrice ,
                request.BrokerAccountId,
                request.UserId
            );

            if (!alertSuccess)
            {
                await _logService.LogAsync(request.UserId, request.StrategyId, request.BrokerAccountId, "Alarm Oluşturmma", "Hata", strategy.Symbol, "Alarm oluşturulamadı.");
                return new ErrorResult("TradingView'de alert oluşturulamadı.");
            }


            await _logService.LogAsync(request.UserId, request.StrategyId, request.BrokerAccountId, "Alarm Oluşturmma", "Başarılı", strategy.Symbol, "Alarm oluşturulamadı.");
            return new SuccessResult("Strateji ve alert TradingView'de başarıyla oluşturuldu.");
        }


        private string GenerateStrategyScript(Strategy strategy, int quantity)
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
