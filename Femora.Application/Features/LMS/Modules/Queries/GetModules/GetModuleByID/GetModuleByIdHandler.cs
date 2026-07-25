using Femora.Application.Common.Interfaces;
using Femora.Application.Features.LMS.Modules.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace Femora.Application.Features.LMS.Modules.Queries.GetModules.GetModuleByID
{
    public class GetModuleByIdHandler : IRequestHandler<GetModuleByIdQuery, ModuleDto?>
    {
        private readonly IAppDbContext _context;

        public GetModuleByIdHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<ModuleDto?> Handle(GetModuleByIdQuery request, CancellationToken cancellationToken)
        {
            var module = await _context.Modules
                .Where(x => x.Id == request.Id)
                .Select(x => new ModuleDto
                {
                    Id = x.Id,
                    Title = x.Title,
                    OrderIndex = x.OrderIndex,
                    CourseId = x.CourseId
                })
                .FirstOrDefaultAsync(cancellationToken);

            return module;
        }
    }
}
