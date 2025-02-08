using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Domain.Entities;
using MediatR;

namespace AutoTraderApp.Application.Features.OkxCustomStocks.Commands
{
    public class DeleteOkxCustomStockCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
    }

    public class DeleteOkxCustomStockCommandHandler : IRequestHandler<DeleteOkxCustomStockCommand, bool>
    {
        private readonly IBaseRepository<OkxCustomStock> _repository;

        public DeleteOkxCustomStockCommandHandler(IBaseRepository<OkxCustomStock> repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(DeleteOkxCustomStockCommand request, CancellationToken cancellationToken)
        {
            var stock = await _repository.GetByIdAsync(request.Id);
            if (stock == null) return false;

            await _repository.DeleteAsync(stock);
            await _repository.SaveChangesAsync();

            return true;
        }
    }
}
