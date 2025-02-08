using AutoTraderApp.Application.Features.OkxCustomStocks.DTOs;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using MediatR;

namespace AutoTraderApp.Application.Features.OkxCustomStocks.Commands
{
    public class CreateOkxCustomStockCommand : IRequest<IResult>
    {
        public CreateOkxCustomStockDto Dto { get; set; }
    }

    public class CreateOkxCustomStockCommandHandler : IRequestHandler<CreateOkxCustomStockCommand, IResult>
    {
        private readonly IBaseRepository<OkxCustomStock> _repository;

        public CreateOkxCustomStockCommandHandler(IBaseRepository<OkxCustomStock> repository)
        {
            _repository = repository;
        }

        public async Task<IResult> Handle(CreateOkxCustomStockCommand request, CancellationToken cancellationToken)
        {
            var isSymbol = _repository.GetAsync(x => x.Symbol == request.Dto.Symbol).Result;
                 
            if (isSymbol != null)
            {
                return new ErrorResult($"{request.Dto.Symbol} sembolü sistemde mevcut.");
            }
            var stock = new OkxCustomStock
            {
                Symbol = request.Dto.Symbol
            };

            await _repository.AddAsync(stock);
            await _repository.SaveChangesAsync();

            return new SuccessResult($"{request.Dto.Symbol} sembolü başarılıyla kaydedildi.");
        }
    }
}
