using AutoTraderApp.Application.Features.TradingView.DTOs;
using FluentValidation;

namespace AutoTraderApp.Application.Features.TradingView.Commands.SaveSignal;

public class TradingViewSignalValidator : AbstractValidator<TradingViewSignalDto>
{
    public TradingViewSignalValidator()
    {
        RuleFor(x => x.Symbol)
            .NotEmpty().WithMessage("Sembol boş olamaz.");

        RuleFor(x => x.Action)
            .NotEmpty().WithMessage("İşlem türü boş olamaz.")
            .Must(action => action.ToUpper() == "BUY" || action.ToUpper() == "SELL")
            .WithMessage("İşlem türü yalnızca BUY veya SELL olabilir.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Miktar 0'dan büyük olmalıdır.");
    }
}