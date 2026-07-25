using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Domain.Entities.Marketplace;
using Femora.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Application.Features.MarketPlace.Commands.PlaceOrder
{
    public class PlaceOrderCommandHandler(
     IAppDbContext db)
     : IRequestHandler<PlaceOrderCommand, Guid>
    {
        public async Task<Guid> Handle(
            PlaceOrderCommand request,
            CancellationToken cancellationToken)
        {
            var cart = await db.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.ProductVariant)
                .FirstOrDefaultAsync(
                    c => c.UserId == request.UserId,
                    cancellationToken)
                ?? throw new NotFoundException(
                    "Cart",
                    request.UserId.ToString());

            var order = new Order
            {
                UserId = request.UserId,
                Status = OrderStatus.Pending
            };

            foreach (var item in cart.Items)
            {
                order.OrderItems.Add(new OrderItem
                {
                    ProductVariantId = item.ProductVariantId,
                    Quantity = item.Quantity,
                    UnitPrice = item.ProductVariant.Price
                });

                order.TotalAmount +=
                    item.Quantity * item.ProductVariant.Price;
            }

            db.Orders.Add(order);

            // See CheckoutCommandHandler for why this uses ExecuteDeleteAsync instead of
            // RemoveRange(cart.Items) — avoids DbUpdateConcurrencyException if an item was
            // already removed by another in-flight request.
            await db.CartItems
                .Where(i => i.CartId == cart.Id)
                .ExecuteDeleteAsync(cancellationToken);

            await db.SaveChangesAsync(cancellationToken);

            return order.Id;
        }
    }
}
