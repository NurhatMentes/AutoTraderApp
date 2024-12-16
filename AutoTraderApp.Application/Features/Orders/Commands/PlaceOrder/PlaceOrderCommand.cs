using AutoMapper;
using AutoTraderApp.Application.Features.Orders.DTOs;
using AutoTraderApp.Core.Utilities.Repositories;
using AutoTraderApp.Core.Utilities.Results;
using AutoTraderApp.Domain.Entities;
using AutoTraderApp.Domain.ExternalModels.Alpaca.Models;
using AutoTraderApp.Infrastructure.Interfaces;
using MediatR;

namespace AutoTraderApp.Application.Features.Orders.Commands.PlaceOrder;

public class PlaceOrderCommand : IRequest<IResult>
{
    public PlaceOrderDto OrderDto { get; set; } = null!;
}

public class PlaceOrderCommandHandler : IRequestHandler<PlaceOrderCommand, IResult>
{
    private readonly IAlpacaService _alpacaService;
    private readonly IBaseRepository<Order> _orderRepository;
    private readonly IBaseRepository<BrokerAccount> _brokerAccountRepository;
    private readonly IMapper _mapper;

    public PlaceOrderCommandHandler(
        IAlpacaService alpacaService,
        IBaseRepository<Order> orderRepository,
        IBaseRepository<BrokerAccount> brokerAccountRepository,
        IMapper mapper)
    {
        _alpacaService = alpacaService ?? throw new ArgumentNullException(nameof(alpacaService));
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _brokerAccountRepository = brokerAccountRepository ?? throw new ArgumentNullException(nameof(brokerAccountRepository));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<IResult> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
    {
        if (request == null || request.OrderDto == null)
            return new ErrorResult("Geçersiz emir isteği.");

        var brokerAccount = await _brokerAccountRepository.GetAsync(x => x.Id == request.OrderDto.BrokerAccountId);
        if (brokerAccount == null)
            return new ErrorResult("Geçerli bir broker hesabı bulunamadı.");

        var orderRequest = _mapper.Map<OrderRequest>(request.OrderDto);
        if (orderRequest == null)
            return new ErrorResult("Emir isteği oluşturulamadı.");

        try
        {
            var alpacaResponse = await _alpacaService.PlaceOrderAsync(brokerAccount.UserId, orderRequest);

            if (alpacaResponse.Status == "new"
                || alpacaResponse.Status == "partially_filled"
                || alpacaResponse.Status == "filled"
                || alpacaResponse.Status == "accepted"
                || alpacaResponse.Status == "pending_new")
            {
                var order = _mapper.Map<Order>(alpacaResponse);
                order.BrokerAccountId = brokerAccount.Id; 
                order.CreatedAt = DateTime.UtcNow;

                await _orderRepository.AddAsync(order);
                await _orderRepository.SaveChangesAsync();

                return new SuccessResult("Emir başarıyla oluşturuldu.");
            }

            return new ErrorResult($"Emir başarısız: {alpacaResponse.ErrorMessage}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("İşlem sırasında bir hata oluştu: {ex.Message}");
            return new ErrorResult("İşlem sırasında bir hata oluştu");
        }
    }
}
