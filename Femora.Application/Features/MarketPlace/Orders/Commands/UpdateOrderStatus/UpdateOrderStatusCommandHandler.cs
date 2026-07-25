using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Domain.Entities.Marketplace;
using Femora.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.MarketPlace.Orders.Commands.UpdateOrderStatus
{
    public class UpdateOrderStatusCommandHandler(
        IAppDbContext db,
        ICurrentUserService currentUser)
     : IRequestHandler<UpdateOrderStatusCommand>
    {
        // Seller-allowed forward transitions (can skip intermediate steps)
        private static readonly Dictionary<OrderStatus, OrderStatus[]> AllowedTransitions = new()
        {
            [OrderStatus.Pending]    = [OrderStatus.Processing, OrderStatus.Shipped, OrderStatus.Delivered],
            [OrderStatus.Processing] = [OrderStatus.Shipped, OrderStatus.Delivered],
            [OrderStatus.Shipped]    = [OrderStatus.Delivered],
        };

        public async Task Handle(
            UpdateOrderStatusCommand request,
            CancellationToken cancellationToken)
        {
            if (!Enum.TryParse<OrderStatus>(request.NewStatus, true, out var newStatus))
                throw new InvalidOperationException($"Unknown status '{request.NewStatus}'.");

            var order = await db.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId, cancellationToken);

            if (order is null)
                throw new NotFoundException("Order", request.OrderId.ToString());

            // Verify seller owns at least one item in this order
            var sellerProfile = await db.SellerProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == currentUser.UserId, cancellationToken);

            if (sellerProfile is null)
                throw new UnauthorizedAccessException("Seller profile not found.");

            var sellerProductIds = await db.Products
                .AsNoTracking()
                .Where(p => p.SellerProfileId == sellerProfile.Id)
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);

            var sellerVariantIds = await db.ProductVariants
                .AsNoTracking()
                .Where(v => sellerProductIds.Contains(v.ProductId))
                .Select(v => v.Id)
                .ToListAsync(cancellationToken);

            var sellerOwnsOrder = order.OrderItems
                .Any(oi => sellerVariantIds.Contains(oi.ProductVariantId));

            if (!sellerOwnsOrder)
                throw new UnauthorizedAccessException("You don't have any items in this order.");

            // Enforce allowed transitions
            if (!AllowedTransitions.TryGetValue(order.Status, out var allowed)
                || !allowed.Contains(newStatus))
            {
                throw new InvalidOperationException(
                    $"Cannot change order status from '{order.Status}' to '{newStatus}'.");
            }

            order.Status = newStatus;

            // When an order reaches Delivered, settle the seller's cut for
            // the items that belong to them — mirrors how InstructorEarning
            // rows are created on enrollment.
            if (newStatus == OrderStatus.Delivered)
            {
                await CreateSellerEarningsAsync(order, sellerProfile.Id, sellerVariantIds, cancellationToken);
            }

            await db.SaveChangesAsync(cancellationToken);
        }

        private async Task CreateSellerEarningsAsync(
            Order order,
            Guid sellerProfileId,
            List<Guid> sellerVariantIds,
            CancellationToken cancellationToken)
        {
            var sellerOrderItemIds = order.OrderItems
                .Where(oi => sellerVariantIds.Contains(oi.ProductVariantId))
                .Select(oi => oi.Id)
                .ToList();

            // Idempotency guard: don't double-pay if this ever runs twice
            // for the same order (e.g. retried request).
            var alreadyEarned = await db.SellerEarnings
                .AsNoTracking()
                .Where(e => sellerOrderItemIds.Contains(e.OrderItemId))
                .Select(e => e.OrderItemId)
                .ToListAsync(cancellationToken);

            var newEarnings = order.OrderItems
                .Where(oi => sellerVariantIds.Contains(oi.ProductVariantId)
                             && !alreadyEarned.Contains(oi.Id))
                .Select(oi =>
                {
                    var gross = oi.Quantity * oi.UnitPrice;
                    var fee = SellerEarning.CalculatePlatformFee(gross);
                    return new SellerEarning
                    {
                        SellerProfileId = sellerProfileId,
                        OrderItemId = oi.Id,
                        Amount = gross,
                        PlatformFee = fee,
                        Status = EarningStatus.Pending,
                        EarnedAt = DateTime.UtcNow
                    };
                })
                .ToList();

            if (newEarnings.Count == 0)
                return;

            db.SellerEarnings.AddRange(newEarnings);

            var netTotal = newEarnings.Sum(e => e.Amount - e.PlatformFee);

            // Fetch a tracked copy to update the running total on the profile.
            var trackedProfile = await db.SellerProfiles
                .FirstAsync(s => s.Id == sellerProfileId, cancellationToken);
            trackedProfile.TotalEarnings += netTotal;
        }
    }
}
