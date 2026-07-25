using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace Femora.Application.Features.LMS.Modules.Commands.ReorderModule
{

    public class ReorderModuleCommand : IRequest<bool>
    {
        public Guid ModuleId { get; set; }
        public int NewOrderIndex { get; set; }
    }
}
