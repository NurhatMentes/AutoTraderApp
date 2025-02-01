using AutoTraderApp.Application.Features.CryptoCustomStocks.DTOs;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Domain.Entities;
using MediatR;

namespace AutoTraderApp.Application.Features.CryptoCustomStocks.Commands
{
    public class UpdateCryptoCustomStockCommand : IRequest<bool>
    {
        public UpdateCryptoCustomStockDto Dto { get; set; }
    }

    public class UpdateCryptoCustomStockCommandHandler : IRequestHandler<UpdateCryptoCustomStockCommand, bool>
    {
        private readonly IBaseRepository<CryptoCustomStock> _repository;

        public UpdateCryptoCustomStockCommandHandler(IBaseRepository<CryptoCustomStock> repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(UpdateCryptoCustomStockCommand request, CancellationToken cancellationToken)
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
