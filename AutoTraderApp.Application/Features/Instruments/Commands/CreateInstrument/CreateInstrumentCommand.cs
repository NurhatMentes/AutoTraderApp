using AutoTraderApp.Application.Contracts.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Enums;
using MediatR;
using AutoTraderApp.Domain.Entities;
using System.Security.Claims;
using AutoTraderApp.Application.Features.Rules;

namespace AutoTraderApp.Application.Features.Instruments.Commands.CreateInstrument
{
    public class CreateInstrumentCommand : IRequest<IResult>
    {
        public string Symbol { get; set; }
        public string Name { get; set; }
        public InstrumentType Type { get; set; }
        public string Exchange { get; set; }
        public decimal MinTradeAmount { get; set; }
        public decimal MaxTradeAmount { get; set; }
        public decimal PriceDecimalPlaces { get; set; }
    }

    public class CreateInstrumentCommandHandler : IRequestHandler<CreateInstrumentCommand, IResult>
    {
        private readonly IBaseRepository<Instrument> _instrumentRepository;

        public CreateInstrumentCommandHandler(IBaseRepository<Instrument> instrumentRepository)
        {
            _instrumentRepository = instrumentRepository;
        }

        public async Task<IResult> Handle(CreateInstrumentCommand request, CancellationToken cancellationToken)
        {
            var existingInstrument = await _instrumentRepository.GetSingleAsync(i =>
                i.Symbol == request.Symbol && i.Exchange == request.Exchange);

            if (existingInstrument != null)
                return new ErrorResult("Bu sembol zaten mevcut");

            var instrument = new Instrument
            {
                Symbol = request.Symbol,
                Name = request.Name,
                Type = request.Type,
                Exchange = request.Exchange,
                MinTradeAmount = request.MinTradeAmount,
                MaxTradeAmount = request.MaxTradeAmount,
                PriceDecimalPlaces = request.PriceDecimalPlaces,
                Status = InstrumentStatus.Active
            };

            await _instrumentRepository.AddAsync(instrument);
            return new SuccessResult($"{request.Symbol} sembolü başarıyla eklendi");
        }
    }
}
