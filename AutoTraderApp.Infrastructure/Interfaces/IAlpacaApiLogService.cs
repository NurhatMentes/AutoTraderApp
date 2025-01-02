using AutoTraderApp.Domain.Entities;

namespace AutoTraderApp.Infrastructure.Interfaces
{
    public interface IAlpacaApiLogService
    {
        Task LogAsync(AlpacaApiLog log);
    }
}