using AutoTraderApp.Application.Features.Orders.DTOs;
using FluentValidation;

namespace AutoTraderApp.Application.Features.Orders.Commands.PlaceOrder;

public class PlaceOrderValidator : AbstractValidator<PlaceOrderDto>
{
    public PlaceOrderValidator()
    {
        RuleFor(x => x.Symbol)
            .NotEmpty().WithMessage("Hisse sembolü boş olamaz.");

        RuleFor(x => x.Qty)
            .NotEmpty().WithMessage("Miktar belirtilmelidir.")
            .GreaterThan(0).WithMessage("Miktar 0'dan büyük olmalıdır.");

        RuleFor(x => x.Side)
            .Must(side => side == "buy" || side == "Buy" || side == "BUY" 
                          || side == "sell" || side == "Sell" || side == "SELL")
            .WithMessage("Geçerli bir işlem türü giriniz (buy/sell).");

        RuleFor(x => x.Type)
            .Must(type => new[]
            {
                "market", "limit", "stop", "stop limit", "trailing stop",
                "Market", "Limit", "Stop", "Stop Limit", "Trailing Stop",
                "MARKET", "LIMIT", "STOP", "STOP LIMIT", "TRAILING STOP"
            }.Contains(type))
            .WithMessage("Geçerli bir emir tipi giriniz.");

        RuleFor(x => x.TimeInForce)
            .NotEmpty().WithMessage("'time_in_force' değeri boş olamaz.")
            .Must(time => new[]
            {
                "day", "gtc", "opg", "ioc", "fok",
                "Day", "GTC", "OPG", "IOC", "FOK",
                "DAY", "GTC", "OPG", "IOC", "FOK"
            }.Contains(time))
            .WithMessage("'time_in_force' değeri geçerli bir formatta olmalıdır.");
    }
}