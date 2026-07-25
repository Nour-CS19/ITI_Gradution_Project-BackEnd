using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Application.Features.Identity.Commands.Profile;
public class SelectProfileCommandValidator : AbstractValidator<SelectProfileCommand>
{
    public SelectProfileCommandValidator()
    {
        RuleFor(x => x.Profile).IsInEnum()
            .WithMessage("Invalid profile type.");
    }
}
