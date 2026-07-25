using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Femora.Application.Features.LMS.Modules.Commands.DeleteModule
{
    public record DeleteModuleCommand(Guid Id) : IRequest<bool>;
}
