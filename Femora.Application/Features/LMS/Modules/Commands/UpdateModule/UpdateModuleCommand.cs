using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Application.Features.LMS.Modules.Commands.UpdateModule
{
    public class UpdateModuleCommand : IRequest<bool>
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public int OrderIndex { get; set; }
    }
}
