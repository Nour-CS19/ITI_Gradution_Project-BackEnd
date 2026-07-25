using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Application.Features.Approvals.Commands.ApplySeller
{
 /*   internal class ApplySellerCommand
    {
    }

    using MediatR;

namespace Femora.Application.Features.Approvals.Commands.ApplySeller;*/

    public class ApplySellerCommand : IRequest<Guid>
    {
        public Guid UserId { get; set; }
        public string ShopName { get; set; } = string.Empty;
        public string? Description { get; set; }
    }
}
