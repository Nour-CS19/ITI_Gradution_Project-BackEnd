using Femora.Application.Common.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Femora.Application.Features.LMS.Modules.Commands.ReorderModule
{

    public class ReorderModuleHandler : IRequestHandler<ReorderModuleCommand, bool>
    {
        private readonly IAppDbContext _context;

        public async Task<bool> Handle(ReorderModuleCommand request, CancellationToken cancellationToken)
        {
            var module = await _context.Modules.FindAsync(request.ModuleId);

            if (module == null)
                return false;

            module.OrderIndex = request.NewOrderIndex;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
