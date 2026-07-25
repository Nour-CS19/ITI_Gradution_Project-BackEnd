using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Application.Features.MarketPlace.Commands.RemoveFromCart
{
    public record RemoveFromCartCommand : IRequest
    {
        public Guid CartItemId { get; init; }
    }
}
// NOTE: UserId is intentionally NOT part of this command. It's derived server-side
// from the authenticated request via ICurrentUserService, so a request can only ever
// remove an item from *its own* cart — see RemoveFromCartCommandHandler.
