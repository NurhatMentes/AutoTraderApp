using FluentValidation;

namespace AutoTraderApp.Application.Features.Strategies.Commands.DeleteStrategy;

public class DeleteStrategyCommandValidator : AbstractValidator<DeleteStrategyCommand>
{
    public DeleteStrategyCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Strateji ID'si boş olamaz.");
    }
}