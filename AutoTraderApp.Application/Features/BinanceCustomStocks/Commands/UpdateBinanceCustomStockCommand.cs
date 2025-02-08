using AutoTraderApp.Application.Features.CryptoCustomStocks.DTOs;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Domain.Entities;
using MediatR;

namespace AutoTraderApp.Application.Features.BinanceCustomStocks.Commands
{
    public class UpdateBinanceCustomStockCommand : IRequest<bool>
    {
        public UpdateCryptoCustomStockDto Dto { get; set; }
    }

    public class UpdateCryptoCustomStockCommandHandler : IRequestHandler<UpdateBinanceCustomStockCommand, bool>
    {
        private readonly IBaseRepository<BinanceCustomStock> _repository;

        public UpdateCryptoCustomStockCommandHandler(IBaseRepository<BinanceCustomStock> repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(UpdateBinanceCustomStockCommand request, CancellationToken cancellationToken)
        {
            var stock = await _repository.GetByIdAsync(request.Dto.Id);
            if (stock == null) return false;

            stock.Symbol = request.Dto.Symbol;
            stock.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(stock);
            await _repository.SaveChangesAsync();

            return true;
        }
    }
}
