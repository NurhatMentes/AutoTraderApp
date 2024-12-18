using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Domain.Entities;

namespace AutoTraderApp.Core.Utilities.Services
{
    public class TradingViewLogService
    {
        private readonly IBaseRepository<TradingViewLog> _logRepository;

        public TradingViewLogService(IBaseRepository<TradingViewLog> logRepository)
        {
            _logRepository = logRepository;
        }

        public async Task LogAsync(Guid userId, Guid strategyId, Guid brokerAccountId, string step, string status, string symbol, string message)
        {
            var log = new TradingViewLog
            {
                UserId = userId,
                StrategyId = strategyId,
                BrokerAccountId = brokerAccountId,
                Step = step,
                Status = status,
                Symbol = symbol,
                Message = message
            };

            await _logRepository.AddAsync(log);
            await _logRepository.SaveChangesAsync();
        }
    }
}
