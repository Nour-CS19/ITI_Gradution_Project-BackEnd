using Femora.Application.Common.Interfaces;
using Femora.Application.Features.LMS.Modules.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.LMS.Modules.Queries.GetModules.GetModuleByCourse
{
    public class GetModulesByCourseHandler
        : IRequestHandler<GetModulesByCourseQuery, List<ModuleDto>>
    {
        private readonly IAppDbContext _context;

        public GetModulesByCourseHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<List<ModuleDto>> Handle(
            GetModulesByCourseQuery request,
            CancellationToken cancellationToken)
        {
            return await _context.Modules
                .Where(x => x.CourseId == request.CourseId)
                .OrderBy(x => x.OrderIndex)
                .Select(x => new ModuleDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    OrderIndex = x.OrderIndex
                })
                .ToListAsync(cancellationToken);
        }
    }
}