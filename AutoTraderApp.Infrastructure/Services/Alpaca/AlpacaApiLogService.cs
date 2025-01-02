using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Infrastructure.Interfaces;


namespace AutoTraderApp.Infrastructure.Services.Alpaca
{
    public class AlpacaApiLogService : IAlpacaApiLogService
    {
        private readonly IBaseRepository<AlpacaApiLog> _logRepository;

        public AlpacaApiLogService(IBaseRepository<AlpacaApiLog> logRepository)
        {
            _logRepository = logRepository;
        }

        public async Task LogAsync(AlpacaApiLog log)
        {
            await _logRepository.AddAsync(log);
        }
    }
}