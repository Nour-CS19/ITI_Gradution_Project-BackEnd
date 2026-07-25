using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Application.Features.LMS.Modules.Commands.ReorderModule
{
    public class ReorderModuleCommandValidator : AbstractValidator<ReorderModuleCommand>
    {
        public ReorderModuleCommandValidator()
        {
            RuleFor(x => x.ModuleId)
                .NotEmpty();

            RuleFor(x => x.NewOrderIndex)
                .GreaterThan(0);
        }
    }
}
