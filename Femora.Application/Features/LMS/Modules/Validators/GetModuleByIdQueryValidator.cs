using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Femora.Application.Features.LMS.Modules.Queries.GetModules.GetModuleByID;
using FluentValidation;
namespace Femora.Application.Features.LMS.Modules.Validators
{

    public class GetModuleByIdQueryValidator : AbstractValidator<GetModuleByIdQuery>
    {
        public GetModuleByIdQueryValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Module Id is required")
                .NotEqual(Guid.Empty).WithMessage("Invalid Module Id");
        }
    }
}
