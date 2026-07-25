using FluentValidation;

namespace Femora.Application.Features.Approvals.Commands.ApplySeller;

public class ApplySellerCommandValidator : AbstractValidator<ApplySellerCommand>
{
    public ApplySellerCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required");

        RuleFor(x => x.ShopName)
            .NotEmpty()
            .MaximumLength(100)
            .WithMessage("ShopName is required and must not exceed 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500)
            .When(x => x.Description != null);
    }
}