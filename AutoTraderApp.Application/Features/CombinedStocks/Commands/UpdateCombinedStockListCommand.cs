using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Infrastructure.Interfaces;
using MediatR;

namespace AutoTraderApp.Application.Features.CombinedStocks.Commands
{
    public class UpdateCombinedStockListCommand : IRequest<bool>
    {
    }

    public class UpdateCombinedStockListCommandHandler : IRequestHandler<UpdateCombinedStockListCommand, bool>
    {
        private readonly IAlphaVantageService _alphaVantageService;
        private readonly IBaseRepository<CombinedStock> _combinedStockRepository;

        public UpdateCombinedStockListCommandHandler(
            IAlphaVantageService alphaVantageService,
            IBaseRepository<CombinedStock> combinedStockRepository)
        {
            _alphaVantageService = alphaVantageService;
            _combinedStockRepository = combinedStockRepository;
        }

        public async Task<bool> Handle(UpdateCombinedStockListCommand request, CancellationToken cancellationToken)
        {
            var gainers = await _alphaVantageService.GetTopGainersAsync();
            var losers = await _alphaVantageService.GetTopLosersAsync();
            var mostActive = await _alphaVantageService.GetMostActiveAsync();

            var combinedStocks = new List<CombinedStock>();

            combinedStocks.AddRange(gainers.Select(x => new CombinedStock
            {
                Symbol = x.Ticker,
                Category = "TopGainers",
                Price = decimal.Parse(x.Price),
                ChangePercentage = ParseChangePercentage(x.ChangePercentage),
                Volume = x.Volume,
                UpdatedAt = DateTime.UtcNow
            }));

            combinedStocks.AddRange(losers.Select(x => new CombinedStock
            {
                Symbol = x.Ticker,
                Category = "TopLosers",
                Price = decimal.Parse(x.Price),
                ChangePercentage = ParseChangePercentage(x.ChangePercentage),
                Volume = x.Volume,
                UpdatedAt = DateTime.UtcNow
            }));

            combinedStocks.AddRange(mostActive.Select(x => new CombinedStock
            {
                Symbol = x.Ticker,
                Category = "MostActive",
                Price = decimal.Parse(x.Price),
                ChangePercentage = ParseChangePercentage(x.ChangePercentage),
                Volume = x.Volume,
                UpdatedAt = DateTime.UtcNow
            }));

            await _combinedStockRepository.ClearAsync(); 
            await _combinedStockRepository.AddRangeAsync(combinedStocks);

            return true;
        }

        private decimal? ParseChangePercentage(string changePercentage)
        {
            if (string.IsNullOrEmpty(changePercentage))
                return null;

            var cleanedValue = changePercentage.Replace("%", "").Replace(",", ".").Trim();

            return decimal.TryParse(cleanedValue, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var parsedValue)
                ? parsedValue
                : (decimal?)null;
        }
    }
}
