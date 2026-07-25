using MediatR;
using System;

namespace Femora.Application.Features.MarketPlace.Commands.AddToCart
{
    public record AddToCartCommand : IRequest<Guid>
    {
        public Guid ProductVariantId { get; init; }

        public int Quantity { get; init; }
    }
}
// NOTE: UserId is intentionally NOT part of this command — it's derived server-side from
// the authenticated request via ICurrentUserService (see AddToCartCommandHandler), so a
// caller can never add items into someone else's cart by passing an arbitrary UserId.
