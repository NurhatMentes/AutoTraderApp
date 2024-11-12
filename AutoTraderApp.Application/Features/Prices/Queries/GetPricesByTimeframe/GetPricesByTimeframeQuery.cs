using AutoMapper;
using AutoTraderApp.Application.Contracts.Repositories;
using AutoTraderApp.Application.Features.Prices.DTOs;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using MediatR;

namespace AutoTraderApp.Application.Features.Prices.Queries.GetPricesByTimeframe
{
    public class GetPricesByTimeframeQuery : IRequest<IDataResult<List<PriceDto>>>
    {
        public Guid InstrumentId { get; set; }
        public TimeFrame TimeFrame { get; set; }
        public int Count { get; set; } = 100; 
    }

    public enum TimeFrame
    {
        Minute,
        FiveMinutes,
        FifteenMinutes,
        ThirtyMinutes,
        Hour,
        FourHours,
        Daily,
        Weekly,
        Monthly
    }

    public class GetPricesByTimeframeQueryHandler : IRequestHandler<GetPricesByTimeframeQuery, IDataResult<List<PriceDto>>>
    {
        private readonly IBaseRepository<Price> _priceRepository;
        private readonly IMapper _mapper;

        public GetPricesByTimeframeQueryHandler(IBaseRepository<Price> priceRepository, IMapper mapper)
        {
            _priceRepository = priceRepository;
            _mapper = mapper;
        }

        public async Task<IDataResult<List<PriceDto>>> Handle(GetPricesByTimeframeQuery request, CancellationToken cancellationToken)
        {
            var endDate = DateTime.UtcNow;
            var startDate = GetStartDateByTimeframe(endDate, request.TimeFrame, request.Count);

            var prices = await _priceRepository.GetListWithStringIncludeAsync(
                predicate: p => p.InstrumentId == request.InstrumentId &&
                p.Timestamp >= startDate &&
                               p.Timestamp <= endDate,
                orderBy: q => q.OrderByDescending(p => p.Timestamp));

            var priceDtos = _mapper.Map<List<PriceDto>>(prices);
            return new SuccessDataResult<List<PriceDto>>(priceDtos, $"Son {request.Count} {request.TimeFrame} verisi getirildi");
        }

        private DateTime GetStartDateByTimeframe(DateTime endDate, TimeFrame timeFrame, int count)
        {
            return timeFrame switch
            {
                TimeFrame.Minute => endDate.AddMinutes(-count),
                TimeFrame.FiveMinutes => endDate.AddMinutes(-count * 5),
                TimeFrame.FifteenMinutes => endDate.AddMinutes(-count * 15),
                TimeFrame.ThirtyMinutes => endDate.AddMinutes(-count * 30),
                TimeFrame.Hour => endDate.AddHours(-count),
                TimeFrame.FourHours => endDate.AddHours(-count * 4),
                TimeFrame.Daily => endDate.AddDays(-count),
                TimeFrame.Weekly => endDate.AddDays(-count * 7),
                TimeFrame.Monthly => endDate.AddMonths(-count),
                _ => endDate.AddDays(-count)
            };
        }
    }
}
