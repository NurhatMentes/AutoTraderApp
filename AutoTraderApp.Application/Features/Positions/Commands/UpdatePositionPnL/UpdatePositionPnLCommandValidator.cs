using FluentValidation;

namespace AutoTraderApp.Application.Features.Positions.Commands.UpdatePositionPnL;

public class UpdatePositionPnLCommandValidator : AbstractValidator<UpdatePositionPnLCommand>
{
    public UpdatePositionPnLCommandValidator()
    {
        RuleFor(x => x.PositionId)
            .NotEmpty()
            .WithMessage("Pozisyon ID boş olamaz");

        RuleFor(x => x.CurrentPrice)
            .GreaterThan(0)
            .WithMessage("Fiyat 0'dan büyük olmalıdır");
    }
}