using Femora.Application.Features.LMS.Modules.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Application.Features.LMS.Modules.Queries.GetModules.GetModuleByCourse
{
    public class GetModulesByCourseQuery : IRequest<List<ModuleDto>>
    {
        public Guid CourseId { get; set; }
    }
}
