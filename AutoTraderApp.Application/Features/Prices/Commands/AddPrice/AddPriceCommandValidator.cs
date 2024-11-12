using FluentValidation;

namespace AutoTraderApp.Application.Features.Prices.Commands.AddPrice;

public class AddPriceCommandValidator : AbstractValidator<AddPriceCommand>
{
    public AddPriceCommandValidator()
    {
        RuleFor(x => x.InstrumentId)
            .NotEmpty()
            .WithMessage("Enstrüman ID'si gereklidir");

        RuleFor(x => x.Timestamp)
            .NotEmpty()
            .WithMessage("Zaman bilgisi gereklidir");

        RuleFor(x => x.Open)
            .GreaterThan(0)
            .WithMessage("Açılış fiyatı 0'dan büyük olmalıdır");

        RuleFor(x => x.High)
            .GreaterThanOrEqualTo(x => x.Open)
            .WithMessage("En yüksek fiyat, açılış fiyatından küçük olamaz")
            .GreaterThanOrEqualTo(x => x.Close)
            .WithMessage("En yüksek fiyat, kapanış fiyatından küçük olamaz");

        RuleFor(x => x.Low)
            .LessThanOrEqualTo(x => x.Open)
            .WithMessage("En düşük fiyat, açılış fiyatından büyük olamaz")
            .LessThanOrEqualTo(x => x.Close)
            .WithMessage("En düşük fiyat, kapanış fiyatından büyük olamaz");

        RuleFor(x => x.Volume)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Hacim 0 veya daha büyük olmalıdır");
    }
}