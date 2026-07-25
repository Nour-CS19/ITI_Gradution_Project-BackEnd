using Femora.Application.Common.Interfaces;
using Femora.Domain.Entities.Marketplace;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Femora.Application.Features.MarketPlace.Commands.AddToCart
{
    public class AddToCartCommandHandler(
        IAppDbContext db,
        ICurrentUserService currentUser) : IRequestHandler<AddToCartCommand, Guid>
    {
        private const int MaxRetries = 3;

        public async Task<Guid> Handle(
            AddToCartCommand request,
            CancellationToken cancellationToken)
        {
            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                try
                {
                    return await AddToCartAsync(request, cancellationToken);
                }
                catch (DbUpdateConcurrencyException) when (attempt < MaxRetries)
                {
                    db.ChangeTracker.Clear();

                    // ننتظر قليلاً قبل المحاولة التالية لتقليل الضغط
                    await Task.Delay(100 * attempt, cancellationToken);
                }
              
            }

            throw new InvalidOperationException("فشل تحديث السلة بعد عدة محاولات");
        }

        private async Task<Guid> AddToCartAsync(
            AddToCartCommand request,
            CancellationToken cancellationToken)
        {
            var userId = currentUser.UserId;

            var cart = await db.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

            if (cart is null)
            {
                cart = new Cart { UserId = userId };
                db.Carts.Add(cart);
            }

            var item = cart.Items
                .FirstOrDefault(i => i.ProductVariantId == request.ProductVariantId);
            if (item is not null)
            {
                // If EF Core is using SQL Server we perform an atomic MERGE to avoid optimistic concurrency conflicts
                // when multiple requests increment the same CartItem concurrently.
                if (db is DbContext efDb && efDb.Database.ProviderName?.Contains("SqlServer", StringComparison.OrdinalIgnoreCase) == true)
                {
                    // Ensure cart persisted (new cart needs to be tracked/inserted so FK constraint is satisfied)
                    if (efDb.Entry(cart).State == EntityState.Added)
                    {
                        await db.SaveChangesAsync(cancellationToken);
                    }

                    await efDb.Database.ExecuteSqlInterpolatedAsync($@"
MERGE INTO CartItems WITH (HOLDLOCK) AS Target
USING (VALUES ({cart.Id}, {request.ProductVariantId}, {request.Quantity})) AS Source (CartId, ProductVariantId, Quantity)
ON Target.CartId = Source.CartId AND Target.ProductVariantId = Source.ProductVariantId
WHEN MATCHED THEN UPDATE SET Quantity = Target.Quantity + Source.Quantity
WHEN NOT MATCHED THEN
    INSERT (Id, CartId, ProductVariantId, Quantity)
    VALUES (NEWID(), Source.CartId, Source.ProductVariantId, Source.Quantity);", cancellationToken);

                    return cart.Id;
                }

                // Fallback to in-memory merge + save for other providers
                item.Quantity += request.Quantity;
                await db.SaveChangesAsync(cancellationToken);
                return cart.Id;
            }
            else
            {
                // If EF Core is using SQL Server we can insert atomically using MERGE as well
                if (db is DbContext efDb && efDb.Database.ProviderName?.Contains("SqlServer", StringComparison.OrdinalIgnoreCase) == true)
                {
                    if (efDb.Entry(cart).State == EntityState.Added)
                    {
                        await db.SaveChangesAsync(cancellationToken);
                    }

                    await efDb.Database.ExecuteSqlInterpolatedAsync($@"
MERGE INTO CartItems WITH (HOLDLOCK) AS Target
USING (VALUES ({cart.Id}, {request.ProductVariantId}, {request.Quantity})) AS Source (CartId, ProductVariantId, Quantity)
ON Target.CartId = Source.CartId AND Target.ProductVariantId = Source.ProductVariantId
WHEN MATCHED THEN UPDATE SET Quantity = Target.Quantity + Source.Quantity
WHEN NOT MATCHED THEN
    INSERT (Id, CartId, ProductVariantId, Quantity)
    VALUES (NEWID(), Source.CartId, Source.ProductVariantId, Source.Quantity);", cancellationToken);

                    return cart.Id;
                }

                cart.Items.Add(new CartItem
                {
                    ProductVariantId = request.ProductVariantId,
                    Quantity = request.Quantity
                });

                await db.SaveChangesAsync(cancellationToken);

                return cart.Id;
            }
        }
    }
}
