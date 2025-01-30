using AutoTraderApp.Application.Features.TradingView.DTOs;
using FluentValidation;

namespace AutoTraderApp.Application.Features.TradingView.Commands.SaveSignal;

public class TradingViewSignalValidator : AbstractValidator<TradingViewSignalDto>
{
    public TradingViewSignalValidator()
    {
        RuleFor(x => x.Symbol)
            .NotEmpty().WithMessage("Sembol alanı boş olamaz.")
            .Length(3, 10).WithMessage("Sembol uzunluğu 3 ile 10 karakter arasında olmalıdır.");

        RuleFor(x => x.Action)
            .NotEmpty().WithMessage("Aksiyon alanı boş olamaz.")
            .Must(action => action.ToUpper() == "BUY" || action.ToUpper() == "SELL")
            .WithMessage("Aksiyon yalnızca 'Al' veya 'Sat' olabilir.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Miktar sıfırdan büyük olmalıdır.");
    }
}