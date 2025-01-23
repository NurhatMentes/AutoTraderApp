using AutoMapper;
using AutoTraderApp.Application.Features.NasdaqStocks.DTOs;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using MediatR;


namespace AutoTraderApp.Application.Features.NasdaqStocks.Queries
{
    public class GetNasdaqStocksQuery : IRequest<IDataResult<List<NasdaqStockDto>>> { }

    public class GetNasdaqStocksQueryHandler : IRequestHandler<GetNasdaqStocksQuery, IDataResult<List<NasdaqStockDto>>> 
    {
        private readonly IBaseRepository<NasdaqStock> _repository;
        private readonly IMapper _mapper;

        public GetNasdaqStocksQueryHandler(IBaseRepository<NasdaqStock> repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<IDataResult<List<NasdaqStockDto>>> Handle(GetNasdaqStocksQuery request, CancellationToken cancellationToken)
        {
            var stocks = await _repository.GetAllAsync();
            if (stocks == null || !stocks.Any())
            {
                return new ErrorDataResult<List<NasdaqStockDto>>(null, "Kombine hisse senedi bulunamadı.");
            }

            var stockDtos = _mapper.Map<List<NasdaqStockDto>>(stocks);
            return new SuccessDataResult<List<NasdaqStockDto>>(stockDtos, "Kombine hisseler başarıyla alındı.");
        }
    }
}
