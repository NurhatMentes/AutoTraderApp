using AutoTraderApp.Application.Features.OkxCustomStocks.DTOs;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Domain.Entities;
using MediatR;

namespace AutoTraderApp.Application.Features.OkxCustomStocks.Commands
{
    public class UpdateOkxCustomStockCommand : IRequest<bool>
    {
        public UpdateOkxCustomStockDto Dto { get; set; }
    }

    public class UpdateOkxCustomStockCommandHandler : IRequestHandler<UpdateOkxCustomStockCommand, bool>
    {
        private readonly IBaseRepository<OkxCustomStock> _repository;

        public UpdateOkxCustomStockCommandHandler(IBaseRepository<OkxCustomStock> repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(UpdateOkxCustomStockCommand request, CancellationToken cancellationToken)
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
