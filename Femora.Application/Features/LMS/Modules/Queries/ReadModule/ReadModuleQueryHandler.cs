using Femora.Application.Common.Interfaces;
using Femora.Application.Features.LMS.Modules.DTOs;
using MediatR;
namespace Femora.Application.Features.LMS.Modules.Queries.ReadModule
{
    public class ReadModuleQueryHandler
    : IRequestHandler<ReadModuleQuery, ModuleDto>
    {
        private readonly IAppDbContext _context;

        public ReadModuleQueryHandler(IAppDbContext context)
        {
            _context = context;
        }

        public async Task<ModuleDto> Handle(ReadModuleQuery request, CancellationToken cancellationToken)
        {
            var module = await _context.Modules.FindAsync(request.Id);

            if (module == null)
                return null;

            return new ModuleDto
            {
                Id = module.Id,
                CourseId = module.CourseId,
                Title = module.Title,
                OrderIndex = module.OrderIndex
            };
        }
    }
}
