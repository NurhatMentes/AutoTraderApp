using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Domain.Entities;

namespace AutoTraderApp.Core.Utilities.Services
{
    public class TradingViewSignalLogService
    {
        private readonly IBaseRepository<TradingViewSignalLog> _logRepository;

        public TradingViewSignalLogService(IBaseRepository<TradingViewSignalLog> logRepository)
        {
            _logRepository = logRepository;
        }

        public async Task LogSignalAsync(Guid userId, Guid brokerAccountId, string action, string symbol, decimal quantity, decimal price, string status, string message)
        {
            var log = new TradingViewSignalLog
            {
                UserId = userId,
                BrokerAccountId = brokerAccountId,
                Action = action,
                Symbol = symbol,
                Quantity = quantity,
                Price = price,
                Status = status,
                Message = message,
                CreatedAt = DateTime.UtcNow
            };

            await _logRepository.AddAsync(log);
        }
    }
}
