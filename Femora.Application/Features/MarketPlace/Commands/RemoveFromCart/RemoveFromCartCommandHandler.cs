using Femora.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Femora.Application.Features.MarketPlace.Commands.RemoveFromCart
{
    public class RemoveFromCartCommandHandler(
        IAppDbContext db,
        ICurrentUserService currentUser)
    : IRequestHandler<RemoveFromCartCommand>
    {
        public async Task Handle(
            RemoveFromCartCommand request,
            CancellationToken cancellationToken)
        {
            // ExecuteDeleteAsync issues a single "DELETE ... WHERE Id = @id AND Cart.UserId = @userId"
            // and does not track/expect a specific row count the way Remove()+SaveChanges() does, so
            // it can never throw DbUpdateConcurrencyException — if the item is already gone (e.g. the
            // cart was just cleared by a completed checkout), this is simply a no-op, which is exactly
            // the outcome the caller wants ("this item should not be in my cart").
            //
            // Scoping by Cart.UserId also closes an IDOR: previously any authenticated user could
            // delete *any* cart item by guessing its id, since ownership was never checked.
            await db.CartItems
                .Where(i => i.Id == request.CartItemId && i.Cart.UserId == currentUser.UserId)
                .ExecuteDeleteAsync(cancellationToken);
        }
    }
}
