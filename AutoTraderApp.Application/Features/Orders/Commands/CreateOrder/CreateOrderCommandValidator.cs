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

        When(x => x.Type == OrderType.Limit, () =>
        {
            RuleFor(x => x.Price)
                .NotNull()
                .GreaterThan(0)
                .WithMessage("Limit emirler için fiyat belirtilmelidir");
        });

        When(x => x.StopLoss.HasValue, () =>
        {
            RuleFor(x => x.StopLoss.Value)
                .GreaterThan(0)
                .WithMessage("Stop loss değeri 0'dan büyük olmalıdır");
        });

        When(x => x.TakeProfit.HasValue, () =>
        {
            RuleFor(x => x.TakeProfit.Value)
                .GreaterThan(0)
                .WithMessage("Take profit değeri 0'dan büyük olmalıdır");
        });
    }
}