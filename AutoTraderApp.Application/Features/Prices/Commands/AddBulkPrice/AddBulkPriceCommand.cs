using AutoTraderApp.Application.Contracts.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using MediatR;

namespace AutoTraderApp.Application.Features.Prices.Commands.AddBulkPrice
{
    public class AddBulkPriceCommand : IRequest<IResult>
    {
        public Guid InstrumentId { get; set; }
        public List<PriceData> Prices { get; set; }

        public class PriceData
        {
            public DateTime Timestamp { get; set; }
            public decimal Open { get; set; }
            public decimal High { get; set; }
            public decimal Low { get; set; }
            public decimal Close { get; set; }
            public decimal Volume { get; set; }
        }
    }

    public class AddBulkPriceCommandHandler : IRequestHandler<AddBulkPriceCommand, IResult>
    {
        private readonly IBaseRepository<Price> _priceRepository;
        private readonly IBaseRepository<Instrument> _instrumentRepository;

        public AddBulkPriceCommandHandler(
            IBaseRepository<Price> priceRepository,
            IBaseRepository<Instrument> instrumentRepository)
        {
            _priceRepository = priceRepository;
            _instrumentRepository = instrumentRepository;
        }

        public async Task<IResult> Handle(AddBulkPriceCommand request, CancellationToken cancellationToken)
        {
            var instrument = await _instrumentRepository.GetByIdAsync(request.InstrumentId);
            if (instrument == null)
                return new ErrorResult("Geçersiz enstrüman");

            var prices = request.Prices.Select(p => new Price
            {
                InstrumentId = request.InstrumentId,
                Timestamp = p.Timestamp,
                Open = p.Open,
                High = p.High,
                Low = p.Low,
                Close = p.Close,
                Volume = p.Volume
            }).ToList();

            foreach (var price in prices)
            {
                await _priceRepository.AddAsync(price);
            }

            return new SuccessResult($"{prices.Count} adet fiyat verisi başarıyla eklendi");
        }
    }
}
