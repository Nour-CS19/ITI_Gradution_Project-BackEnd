using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Femora.Application.Features.MarketPlace.Commands.UpdateCartItemQuantity
{
    public class UpdateCartItemQuantityCommandHandler(
        IAppDbContext db,
        ICurrentUserService currentUser)
        : IRequestHandler<UpdateCartItemQuantityCommand>
    {
        public async Task Handle(
            UpdateCartItemQuantityCommand request,
            CancellationToken cancellationToken)
        {
            var userId = currentUser.UserId;

            // ExecuteUpdateAsync issues a single "UPDATE ... WHERE Id = @id AND Cart.UserId = @userId"
            // directly in the DB — it doesn't load/track the entity first, so there's no
            // load-then-save window for another request to delete the row in between, and it
            // can never throw DbUpdateConcurrencyException. It returns the number of rows
            // actually updated (0 or 1), which lets us report a clean 404 when the item is
            // gone or doesn't belong to this user — instead of a raw 500.
            var updated = await db.CartItems
                .Where(i => i.Id == request.CartItemId && i.Cart.UserId == userId)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(i => i.Quantity, request.Quantity),
                    cancellationToken);

            if (updated == 0)
            {
                throw new NotFoundException("CartItem", request.CartItemId.ToString());
            }
        }
    }
}
