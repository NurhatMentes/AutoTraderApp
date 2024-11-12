using AutoTraderApp.Application.Contracts.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using MediatR;

namespace AutoTraderApp.Application.Features.Prices.Commands.AddPrice
{
    public class AddPriceCommand : IRequest<IResult>
    {
        public Guid InstrumentId { get; set; }
        public DateTime Timestamp { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }
        public decimal Volume { get; set; }
    }

    public class AddPriceCommandHandler : IRequestHandler<AddPriceCommand, IResult>
    {
        private readonly IBaseRepository<Price> _priceRepository;
        private readonly IBaseRepository<Instrument> _instrumentRepository;

        public AddPriceCommandHandler(
            IBaseRepository<Price> priceRepository,
            IBaseRepository<Instrument> instrumentRepository)
        {
            _priceRepository = priceRepository;
            _instrumentRepository = instrumentRepository;
        }

        public async Task<IResult> Handle(AddPriceCommand request, CancellationToken cancellationToken)
        {
            var instrument = await _instrumentRepository.GetByIdAsync(request.InstrumentId);
            if (instrument == null)
                return new ErrorResult("Geçersiz enstrüman");

            var existingPrice = await _priceRepository.GetSingleAsync(p =>
                p.InstrumentId == request.InstrumentId &&
                p.Timestamp == request.Timestamp);

            if (existingPrice != null)
                return new ErrorResult("Bu zaman damgası için fiyat verisi zaten mevcut");

            var price = new Price
            {
                InstrumentId = request.InstrumentId,
                Timestamp = request.Timestamp,
                Open = request.Open,
                High = request.High,
                Low = request.Low,
                Close = request.Close,
                Volume = request.Volume
            };

            await _priceRepository.AddAsync(price);
            return new SuccessResult("Fiyat verisi başarıyla eklendi");
        }
    }
}
