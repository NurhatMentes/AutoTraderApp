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

        // Market ve Limit emirler için farklı kurallar
        When(x => x.Type == OrderType.Limit, () =>
        {
            RuleFor(x => x.Price)
                .NotNull()
                .GreaterThan(0)
                .WithMessage("Limit emirler için fiyat belirtilmelidir");
        });

        When(x => x.Type == OrderType.Market, () =>
        {
            RuleFor(x => x.Price)
                .Null()
                .WithMessage("Market emirler için fiyat belirtilmemelidir");
        });

        // Stop Loss ve Take Profit opsiyonel ama belirtilirse kontrol
        When(x => x.StopLoss.HasValue, () =>
        {
            RuleFor(x => x.StopLoss.Value)
                .GreaterThan(0)
                .WithMessage("Stop loss değeri 0'dan büyük olmalıdır");

            // Alış emirlerinde stop loss, fiyatın altında olmalı
            When(x => x.Side == OrderSide.Buy && x.Price.HasValue, () =>
            {
                RuleFor(x => x.StopLoss.Value)
                    .LessThan(x => x.Price.Value)
                    .WithMessage("Alış emirlerinde stop loss, fiyatın altında olmalıdır");
            });
        });

        When(x => x.TakeProfit.HasValue, () =>
        {
            RuleFor(x => x.TakeProfit.Value)
                .GreaterThan(0)
                .WithMessage("Take profit değeri 0'dan büyük olmalıdır");

            // Alış emirlerinde take profit, fiyatın üstünde olmalı
            When(x => x.Side == OrderSide.Buy && x.Price.HasValue, () =>
            {
                RuleFor(x => x.TakeProfit.Value)
                    .GreaterThan(x => x.Price.Value)
                    .WithMessage("Alış emirlerinde take profit, fiyatın üstünde olmalıdır");
            });
        });
    }
}
