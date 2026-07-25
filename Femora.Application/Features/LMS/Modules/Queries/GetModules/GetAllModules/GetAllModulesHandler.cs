using Femora.Application.Common.Interfaces;
using Femora.Application.Features.LMS.Modules.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Application.Features.LMS.Modules.Queries.GetModules.GetAllModules
{
    public class GetAllModulesHandler : IRequestHandler<GetAllModulesQuery, List<ModuleDto>>
    {
        private readonly IAppDbContext _context;

        public GetAllModulesHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ModuleDto>> Handle(GetAllModulesQuery request, CancellationToken cancellationToken)
        {
            return await _context.Modules
                .OrderBy(x => x.CourseId)
                .ThenBy(x => x.OrderIndex)
                .Select(x => new ModuleDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    OrderIndex = x.OrderIndex,
                    CourseId = x.CourseId
                })
                .ToListAsync(cancellationToken);
        }
    }
}
