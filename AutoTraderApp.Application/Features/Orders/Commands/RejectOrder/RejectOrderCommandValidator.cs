using FluentValidation;

namespace AutoTraderApp.Application.Features.Orders.Commands.RejectOrder;

public class RejectOrderCommandValidator : AbstractValidator<RejectOrderCommand>
{
    public RejectOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("Emir ID'si gereklidir");

        RuleFor(x => x.RejectionReason)
            .NotEmpty()
            .WithMessage("Red nedeni belirtilmelidir")
            .MaximumLength(500)
            .WithMessage("Red nedeni en fazla 500 karakter olabilir");
    }
}