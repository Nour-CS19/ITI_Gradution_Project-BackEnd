using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Femora.Application.Features.LMS.Modules.Queries.GetModules.GetModuleByCourse;
using FluentValidation;

namespace Femora.Application.Features.LMS.Modules.Validators
{

    public class GetModulesByCourseIdQueryValidator : AbstractValidator<GetModulesByCourseQuery>
    {
        public GetModulesByCourseIdQueryValidator()
        {
            RuleFor(x => x.CourseId)
                .NotEmpty().WithMessage("Course Id is required")
                .NotEqual(Guid.Empty).WithMessage("Invalid Course Id");
        }
    }
}
