using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using AutoTraderApp.Application.Contracts.Repositories;
using AutoTraderApp.Application.Features.Prices.DTOs;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using MediatR;

namespace AutoTraderApp.Application.Features.Prices.Queries.GetPrices
{
    public class GetPricesQuery : IRequest<IDataResult<List<PriceDto>>>
    {
        public Guid InstrumentId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Limit { get; set; } // Son n kadar veriyi getirmek için
    }

    public class GetPricesQueryHandler : IRequestHandler<GetPricesQuery, IDataResult<List<PriceDto>>>
    {
        private readonly IBaseRepository<Price> _priceRepository;
        private readonly IMapper _mapper;

        public GetPricesQueryHandler(IBaseRepository<Price> priceRepository, IMapper mapper)
        {
            _priceRepository = priceRepository;
            _mapper = mapper;
        }

        public async Task<IDataResult<List<PriceDto>>> Handle(GetPricesQuery request, CancellationToken cancellationToken)
        {
            var query = await _priceRepository.GetListWithStringIncludeAsync(
                predicate: p => p.InstrumentId == request.InstrumentId &&
                                (!request.StartDate.HasValue || p.Timestamp >= request.StartDate) &&
                                (!request.EndDate.HasValue || p.Timestamp <= request.EndDate),
                orderBy: q => q.OrderByDescending(p => p.Timestamp));

            var prices = query.ToList();

            if (request.Limit.HasValue && request.Limit.Value > 0)
            {
                prices = prices.Take(request.Limit.Value).ToList();
            }

            var priceDtos = _mapper.Map<List<PriceDto>>(prices);
            return new SuccessDataResult<List<PriceDto>>(priceDtos, "Fiyat verileri başarıyla getirildi");
        }
    }
}
