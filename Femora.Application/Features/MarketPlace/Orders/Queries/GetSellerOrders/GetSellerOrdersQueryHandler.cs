using Femora.Application.Common.Interfaces;
using Femora.Application.Common.Models;
using Femora.Application.Features.MarketPlace.Orders.DTOs;
using Femora.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.MarketPlace.Orders.Queries.GetSellerOrders
{
    public class GetSellerOrdersQueryHandler(
        IAppDbContext db,
        ICurrentUserService currentUser)
     : IRequestHandler<GetSellerOrdersQuery, PagedResult<SellerOrderDto>>
    {
        public async Task<PagedResult<SellerOrderDto>> Handle(
            GetSellerOrdersQuery request,
            CancellationToken cancellationToken)
        {
            // Resolve seller profile from the authenticated user
            var sellerProfile = await db.SellerProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == currentUser.UserId, cancellationToken);

            if (sellerProfile is null)
                throw new UnauthorizedAccessException("Seller profile not found.");

            // Find product IDs owned by this seller
            var sellerProductIds = await db.Products
                .AsNoTracking()
                .Where(p => p.SellerProfileId == sellerProfile.Id)
                .Select(p => p.Id)
                .ToListAsync(cancellationToken);

            // Find variant IDs belonging to those products
            var sellerVariantIds = await db.ProductVariants
                .AsNoTracking()
                .Where(v => sellerProductIds.Contains(v.ProductId))
                .Select(v => v.Id)
                .ToListAsync(cancellationToken);

            // Orders that have at least one item from this seller
            var query = db.Orders
                .AsNoTracking()
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(v => v!.Product)
                .Where(o => o.OrderItems.Any(oi => sellerVariantIds.Contains(oi.ProductVariantId)));

            // Status filter
            if (!string.IsNullOrWhiteSpace(request.Status)
                && Enum.TryParse<OrderStatus>(request.Status, true, out var statusEnum))
            {
                query = query.Where(o => o.Status == statusEnum);
            }

            // Search: customer name or order number (GUID prefix)
            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var s = request.Search.ToLower();
                query = query.Where(o =>
                    o.User.FirstName.ToLower().Contains(s) ||
                    o.User.LastName.ToLower().Contains(s) ||
                    o.Id.ToString().ToLower().Contains(s));
            }

            var total = await query.CountAsync(cancellationToken);

            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var dtos = orders.Select(o =>
            {
                // Only include items that belong to this seller
                var sellerItems = o.OrderItems
                    .Where(oi => sellerVariantIds.Contains(oi.ProductVariantId))
                    .Select(oi => new SellerOrderItemDto(
                        ProductVariantId: oi.ProductVariantId,
                        ProductName: oi.ProductVariant?.Product?.Name ?? "—",
                        VariantName: oi.ProductVariant?.Name ?? "—",
                        Quantity: oi.Quantity,
                        UnitPrice: oi.UnitPrice,
                        LineTotal: oi.Quantity * oi.UnitPrice
                    )).ToList();

                var sellerTotal = sellerItems.Sum(i => i.LineTotal);

                return new SellerOrderDto(
                    Id: o.Id,
                    OrderNumber: o.Id.ToString()[..8].ToUpper(),
                    CustomerFirstName: o.User?.FirstName ?? "—",
                    CustomerLastName: o.User?.LastName ?? "",
                    Status: o.Status.ToString(),
                    TotalAmount: sellerTotal,
                    CreatedAt: o.CreatedAt,
                    Items: sellerItems
                );
            }).ToList();

            return new PagedResult<SellerOrderDto>
            {
                Items = dtos,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = total
            };
        }
    }
}
