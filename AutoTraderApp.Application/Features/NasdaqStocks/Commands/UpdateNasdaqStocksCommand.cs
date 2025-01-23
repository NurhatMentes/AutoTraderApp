using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Infrastructure.Interfaces;
using MediatR;

namespace AutoTraderApp.Application.Features.NasdaqStocks.Commands
{
    public class UpdateNasdaqStocksCommand : IRequest<bool>
    {
    }

    public class UpdateNasdaqStocksCommandHandler : IRequestHandler<UpdateNasdaqStocksCommand, bool>
    {
        private readonly IAlphaVantageService _alphaVantageService;
        private readonly IBaseRepository<NasdaqStock> _nasdaqStocksRepository;

        public UpdateNasdaqStocksCommandHandler(
            IAlphaVantageService alphaVantageService,
            IBaseRepository<NasdaqStock> nasdaqStocks)
        {
            _alphaVantageService = alphaVantageService;
            _nasdaqStocksRepository = nasdaqStocks;
        }

        public async Task<bool> Handle(UpdateNasdaqStocksCommand request, CancellationToken cancellationToken)
        {
            var stockListings = await _alphaVantageService.GetNasdaqListingsAsync(600);

            var stocks = new List<NasdaqStock>();

            stocks.AddRange(stockListings.Select(x => new NasdaqStock
            {
                Symbol = x.Symbol,
                Name = x.Name,
                AssetType = x.AssetType,
                DelistingDate = x.DelistingDate,
                Exchange = x.Exchange,
                IpoDate = x.IpoDate,
                Status = x.Status,
                UpdatedAt = DateTime.UtcNow
            }));


            await _nasdaqStocksRepository.ClearAsync();
            await _nasdaqStocksRepository.AddRangeAsync(stocks);


            return true;
        }

    }
}
