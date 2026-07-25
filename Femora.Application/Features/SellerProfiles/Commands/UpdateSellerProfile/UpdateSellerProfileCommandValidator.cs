using FluentValidation;

namespace Femora.Application.Features.SellerProfiles.Commands.UpdateSellerProfile
{
    public class UpdateSellerProfileCommandValidator
        : AbstractValidator<UpdateSellerProfileCommand>
    {
        public UpdateSellerProfileCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty();

            RuleFor(x => x.StoreName)
                .NotEmpty()
                .MaximumLength(200);

            RuleFor(x => x.StoreDescription)
                .MaximumLength(1000);

            RuleFor(x => x.BusinessAddress)
                .MaximumLength(300);

            RuleFor(x => x.BusinessPhone)
                .MaximumLength(30);

            RuleFor(x => x.ContactEmail)
                .MaximumLength(200)
                .EmailAddress()
                .When(x => !string.IsNullOrWhiteSpace(x.ContactEmail));

            RuleFor(x => x.TaxId)
                .MaximumLength(50);
        }
    }
}
