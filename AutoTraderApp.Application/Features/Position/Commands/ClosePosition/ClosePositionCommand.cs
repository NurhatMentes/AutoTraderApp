using AutoMapper;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Infrastructure.Interfaces;
using MediatR;

namespace AutoTraderApp.Application.Features.Position.Commands.ClosePosition
{
    public class ClosePositionCommand : IRequest<IResult>
    {
        public Guid BrokerAccountId { get; set; }
        public string Symbol { get; set; }
        public decimal? Quantity { get; set; }
    }

    public class ClosePositionCommandHandler : IRequestHandler<ClosePositionCommand, IResult>
    {
        private readonly IAlpacaService _alpacaService;

        public ClosePositionCommandHandler(IAlpacaService alpacaService)
        {
            _alpacaService = alpacaService ?? throw new ArgumentNullException(nameof(alpacaService));
        }

        public async Task<IResult> Handle(ClosePositionCommand request, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Pozisyon kapatma işlemi başlıyor. Symbol: {request.Symbol}, BrokerAccountId: {request.BrokerAccountId}");

            var closeResult = await _alpacaService.ClosePositionAsync(request.Symbol, request.Quantity, request.BrokerAccountId);

            if (!closeResult.Success)
                return new ErrorResult($"Pozisyon kapatma sırasında hata: {closeResult.Message}");

            Console.WriteLine($"Pozisyon başarıyla kapatıldı: {request.Symbol}");
            return new SuccessResult($"Pozisyon başarıyla kapatıldı: {request.Symbol}");
        }


    }

}
