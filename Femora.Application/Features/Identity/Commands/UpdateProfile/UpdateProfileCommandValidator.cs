using FluentValidation;
using System;
using System.Linq;

namespace Femora.Application.Features.Identity.Commands.UpdateProfile;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    private static readonly string[] AllowedAvatarTypes = { "image/jpeg", "image/png", "image/webp" };
    private const long MaxAvatarSizeBytes = 5 * 1024 * 1024; // 5 MB

    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20)
            .Matches(@"^\+?[0-9\s\-]+$")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber))
            .WithMessage("PhoneNumber must be a valid phone number");

        RuleFor(x => x.Bio)
            .MaximumLength(1000);

        RuleFor(x => x.LinkedInUrl)
            .MaximumLength(300)
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.LinkedInUrl))
            .WithMessage("LinkedInUrl must be a valid URL");

        RuleFor(x => x.GitHubUrl)
            .MaximumLength(300)
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .When(x => !string.IsNullOrWhiteSpace(x.GitHubUrl))
            .WithMessage("GitHubUrl must be a valid URL");

        RuleFor(x => x.Country)
            .MaximumLength(100);

        RuleFor(x => x.Avatar!.Length)
            .LessThanOrEqualTo(MaxAvatarSizeBytes)
            .WithMessage("Avatar image must not exceed 5MB")
            .When(x => x.Avatar is not null);

        RuleFor(x => x.Avatar!.ContentType)
            .Must(ct => AllowedAvatarTypes.Contains(ct))
            .WithMessage("Avatar must be a JPEG, PNG, or WEBP image")
            .When(x => x.Avatar is not null);
    }
}
