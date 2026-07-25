using MediatR;
using System;

namespace Femora.Application.Features.MarketPlace.Commands.UpdateCartItemQuantity
{
    // UserId is intentionally NOT part of this command — it's derived server-side from
    // the authenticated request (ICurrentUserService), so a caller can only ever update
    // an item that belongs to their own cart.
    public record UpdateCartItemQuantityCommand : IRequest
    {
        public Guid CartItemId { get; init; }

        public int Quantity { get; init; }
    }
}
