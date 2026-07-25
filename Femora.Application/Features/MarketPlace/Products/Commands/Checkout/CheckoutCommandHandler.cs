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

namespace Femora.Application.Features.MarketPlace.Products.Commands.Checkout
{
    public class CheckoutCommandHandler(IAppDbContext db)
     : IRequestHandler<CheckoutCommand, Guid>
    {
        public async Task<Guid> Handle(
            CheckoutCommand request,
            CancellationToken cancellationToken)
        {
            var cart = await db.Carts
                .Include(c => c.Items)
                    .ThenInclude(i => i.ProductVariant)
                .FirstOrDefaultAsync(
                    c => c.UserId == request.UserId,
                    cancellationToken);

            if (cart is null || !cart.Items.Any())
            {
                throw new NotFoundException(
                    "Cart",
                    request.UserId.ToString());
            }

            decimal totalAmount = cart.Items.Sum(x =>
                x.Quantity * x.ProductVariant.Price);

            var order = new Order
            {
                UserId = request.UserId,
                Status = OrderStatus.Pending,
                TotalAmount = totalAmount,
                OrderItems = cart.Items.Select(x => new OrderItem
                {
                    ProductVariantId = x.ProductVariantId,
                    Quantity = x.Quantity,
                    UnitPrice = x.ProductVariant.Price
                }).ToList()
            };

            db.Orders.Add(order);

            await db.SaveChangesAsync(cancellationToken);

            var payment = new Payment
            {
                OrderId = order.Id,
                Amount = totalAmount,
                PaymentMethod = "Mock",
                PaymentStatus = "Paid",
                TransactionReference = Guid.NewGuid().ToString()
            };

            db.Payments.Add(payment);

            db.CartItems.RemoveRange(cart.Items);

            await db.SaveChangesAsync(cancellationToken);

            return order.Id;
        }
    }
}
