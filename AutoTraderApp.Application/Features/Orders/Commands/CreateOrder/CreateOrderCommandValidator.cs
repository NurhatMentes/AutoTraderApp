using AutoTraderApp.Domain.Enums;
using FluentValidation;

namespace AutoTraderApp.Application.Features.Orders.Commands.CreateOrder;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.InstrumentId)
            .NotEmpty()
            .WithMessage("Enstrüman seçimi zorunludur");

        RuleFor(x => x.BrokerAccountId)
            .NotEmpty()
            .WithMessage("Broker hesabı seçimi zorunludur");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Miktar 0'dan büyük olmalıdır");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Geçerli bir emir tipi seçiniz");

        RuleFor(x => x.Side)
            .IsInEnum()
            .WithMessage("Geçerli bir yön seçiniz");

        // Market emirleri için
        When(x => x.Type == OrderType.Market, () =>
        {
            RuleFor(x => x.Price)
                .Null()
                .WithMessage("Market emirlerde fiyat belirtilmemelidir");
        });

        // Limit emirleri için
        When(x => x.Type == OrderType.Limit, () =>
        {
            RuleFor(x => x.Price)
                .NotNull()
                .WithMessage("Limit emirlerde fiyat belirtilmelidir")
                .GreaterThan(0)
                .WithMessage("Limit fiyatı 0'dan büyük olmalıdır");
        });

        // Stop Loss opsiyonel ama varsa
        When(x => x.StopLoss.HasValue, () =>
        {
            RuleFor(x => x.StopLoss.Value)
                .GreaterThan(0)
                .WithMessage("Stop loss değeri 0'dan büyük olmalıdır");
        });

        // Take Profit opsiyonel ama varsa
        When(x => x.TakeProfit.HasValue, () =>
        {
            RuleFor(x => x.TakeProfit.Value)
                .GreaterThan(0)
                .WithMessage("Take profit değeri 0'dan büyük olmalıdır");
        });
    }
}

