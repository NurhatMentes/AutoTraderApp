using AutoTraderApp.Application.Features.CustomStocks.DTOs;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Domain.Entities;
using MediatR;

namespace AutoTraderApp.Application.Features.CustomStocks.Commands
{
    public class CreateCustomStockCommand : IRequest<Guid>
    {
        public CreateCustomStockDto Dto { get; set; }
    }

    public class CreateCustomStockCommandHandler : IRequestHandler<CreateCustomStockCommand, Guid>
    {
        private readonly IBaseRepository<CustomStock> _repository;

        public CreateCustomStockCommandHandler(IBaseRepository<CustomStock> repository)
        {
            _repository = repository;
        }

        public async Task<Guid> Handle(CreateCustomStockCommand request, CancellationToken cancellationToken)
        {
            var stock = new CustomStock
            {
                Symbol = request.Dto.Symbol,
            };

            await _repository.AddAsync(stock);
            await _repository.SaveChangesAsync();

            return stock.Id;
        }
    }

}
