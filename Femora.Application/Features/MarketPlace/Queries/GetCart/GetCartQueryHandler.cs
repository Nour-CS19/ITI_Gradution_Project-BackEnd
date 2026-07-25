using Femora.Application.Common.Interfaces;
using Femora.Application.Features.MarketPlace.Dtos;
using Femora.Domain.Entities.Marketplace;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.MarketPlace.Queries.GetCart;

public class GetCartQueryHandler(IAppDbContext db)
    : IRequestHandler<GetCartQuery, CartDto>
{
    public async Task<CartDto> Handle(GetCartQuery request, CancellationToken cancellationToken)
    {
        var cart = await db.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.ProductVariant)
                    .ThenInclude(v => v.Product)
                        .ThenInclude(p => p.ProductImages)
            .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);

        if (cart is null)
        {
            // Ensure the user exists before creating a cart to avoid FK constraint violations
            var userExists = await db.ApplicationUsers.AnyAsync(u => u.Id == request.UserId, cancellationToken);
            if (!userExists)
                throw new Femora.Application.Common.Exceptions.NotFoundException("ApplicationUser", request.UserId.ToString());

            cart = new Cart { UserId = request.UserId };
            await db.Carts.AddAsync(cart, cancellationToken);
            await db.SaveChangesAsync(cancellationToken);
        }

        return MapToDto(cart);
    }

    // Flattens the entity graph into the DTO the frontend actually reads pricing/name/image
    // from — see CartDto.cs for why this mapping step is required.
    private static CartDto MapToDto(Cart cart)
    {
        var items = cart.Items
            .Select(i =>
            {
                var unitPrice = i.ProductVariant?.Price ?? 0m;
                var lineTotal = unitPrice * i.Quantity;
                var primaryImage = i.ProductVariant?.Product?.ProductImages
                    ?.Where(img => img.IsPrimary)
                    .Select(img => img.ImageUrl)
                    .FirstOrDefault()
                    ?? i.ProductVariant?.Product?.ProductImages?.Select(img => img.ImageUrl).FirstOrDefault();

                return new CartItemDto
                {
                    CartItemId = i.Id,
                    ProductId = i.ProductVariant?.ProductId ?? Guid.Empty,
                    ProductVariantId = i.ProductVariantId,
                    ProductName = i.ProductVariant?.Product?.Name ?? string.Empty,
                    VariantLabel = i.ProductVariant?.Name,
                    ImageUrl = primaryImage,
                    Quantity = i.Quantity,
                    UnitPrice = unitPrice,
                    LineTotal = lineTotal,
                };
            })
            .ToList();

        return new CartDto
        {
            Id = cart.Id,
            Items = items,
            Total = items.Sum(i => i.LineTotal),
        };
    }
}
