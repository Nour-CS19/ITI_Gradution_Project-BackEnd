using Femora.Application.Common.Interfaces;
using Femora.Application.Features.Approvals.Common;
using Femora.Application.Features.MarketPlace.Products.Common;
using Femora.Application.Features.MarketPlace.Products.DTOs;
using Femora.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.MarketPlace.Products.Queries.GetSellerStats
{
    public class GetSellerStatsQueryHandler(
        IAppDbContext db,
        ICurrentUserService currentUser)
     : IRequestHandler<GetSellerStatsQuery, SellerStatsDto>
    {
        public async Task<SellerStatsDto> Handle(
            GetSellerStatsQuery request,
            CancellationToken cancellationToken)
        {
            var sellerProfile = await db.SellerProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == currentUser.UserId, cancellationToken);

            if (sellerProfile is null)
                throw new UnauthorizedAccessException("Seller profile not found.");

            // ── Products ─────────────────────────────────────────────────────
            var products = await db.Products
                .AsNoTracking()
                .Where(p => p.SellerProfileId == sellerProfile.Id)
                .Select(p => new { p.Id, p.IsPuplished, p.Name,
                    ImageUrl = p.ProductImages.Where(i => i.IsPrimary).Select(i => i.ImageUrl).FirstOrDefault() })
                .ToListAsync(cancellationToken);

            var productIds = products.Select(p => p.Id).ToList();

            // Latest approval per product
            var latestApprovals = await db.ApprovalRequests
                .AsNoTracking()
                .Where(a => a.Type == ApprovalEntityType.ProductApproval && productIds.Contains(a.EntityId))
                .OrderByDescending(a => a.RequestedAt)
                .GroupBy(a => a.EntityId)
                .Select(g => new { ProductId = g.Key, Status = g.First().ApprovalStatus })
                .ToDictionaryAsync(x => x.ProductId, x => x.Status, cancellationToken);

            // Compute status for each product
            var productStatuses = products.Select(p =>
            {
                latestApprovals.TryGetValue(p.Id, out var approvalStatus);
                return new
                {
                    p.Id,
                    p.Name,
                    p.ImageUrl,
                    Status = ProductStatusHelper.Resolve(p.IsPuplished, approvalStatus)
                };
            }).ToList();

            // ── Seller variant IDs (for order filtering) ──────────────────────
            var sellerVariantIds = await db.ProductVariants
                .AsNoTracking()
                .Where(v => productIds.Contains(v.ProductId))
                .Select(v => v.Id)
                .ToListAsync(cancellationToken);

            // ── Orders ────────────────────────────────────────────────────────
            var orders = await db.Orders
                .AsNoTracking()
                .Include(o => o.User)
                .Include(o => o.OrderItems)
                .Where(o => o.OrderItems.Any(oi => sellerVariantIds.Contains(oi.ProductVariantId)))
                .ToListAsync(cancellationToken);

            // Revenue: sum of seller item line totals in paid orders
            var revenue = orders
                .Where(o => o.Status != OrderStatus.Cancelled && o.Status != OrderStatus.Pending)
                .SelectMany(o => o.OrderItems.Where(oi => sellerVariantIds.Contains(oi.ProductVariantId)))
                .Sum(oi => oi.Quantity * oi.UnitPrice);

            // ── Best selling products ─────────────────────────────────────────
            // Build variantId → productId lookup from DB (variants are not loaded on OrderItems)
            var variantToProduct = await db.ProductVariants
                .AsNoTracking()
                .Where(v => sellerVariantIds.Contains(v.Id))
                .Select(v => new { v.Id, v.ProductId })
                .ToListAsync(cancellationToken);

            var variantProductMap = variantToProduct.ToDictionary(v => v.Id, v => v.ProductId);

            var salesByProduct = orders
                .SelectMany(o => o.OrderItems.Where(oi => sellerVariantIds.Contains(oi.ProductVariantId)))
                .GroupBy(oi => variantProductMap.TryGetValue(oi.ProductVariantId, out var pid) ? pid : Guid.Empty)
                .Select(g => new
                {
                    ProductId = g.Key,
                    TotalSold = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.Quantity * oi.UnitPrice)
                })
                .OrderByDescending(x => x.TotalSold)
                .Take(4)
                .ToList();

            // We need product names — look them up from the in-memory list
            var productNameMap = productStatuses.ToDictionary(p => p.Id);
            var bestSellers = salesByProduct.Select(s =>
            {
                productNameMap.TryGetValue(s.ProductId, out var prod);
                return new BestSellerProductDto(
                    ProductId: s.ProductId,
                    ProductName: prod?.Name ?? "—",
                    ImageUrl: prod?.ImageUrl,
                    TotalSold: s.TotalSold,
                    Revenue: s.Revenue
                );
            }).ToList();

            // ── Latest orders ─────────────────────────────────────────────────
            var latestOrders = orders
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .Select(o =>
                {
                    var sellerAmount = o.OrderItems
                        .Where(oi => sellerVariantIds.Contains(oi.ProductVariantId))
                        .Sum(oi => oi.Quantity * oi.UnitPrice);

                    return new SellerRecentOrderDto(
                        OrderId: o.Id,
                        OrderNumber: o.Id.ToString()[..8].ToUpper(),
                        CustomerName: $"{o.User?.FirstName} {o.User?.LastName}".Trim(),
                        Status: o.Status.ToString(),
                        Amount: sellerAmount,
                        CreatedAt: o.CreatedAt
                    );
                }).ToList();

            return new SellerStatsDto
            {
                TotalProducts    = productStatuses.Count,
                DraftProducts    = productStatuses.Count(p => p.Status == ProductStatus.Draft),
                PendingProducts  = productStatuses.Count(p => p.Status == ProductStatus.PendingApproval),
                ApprovedProducts = productStatuses.Count(p => p.Status == ProductStatus.Approved),
                RejectedProducts = productStatuses.Count(p => p.Status == ProductStatus.Rejected),

                TotalOrders      = orders.Count,
                PendingOrders    = orders.Count(o => o.Status == OrderStatus.Pending),
                ProcessingOrders = orders.Count(o => o.Status == OrderStatus.Processing),
                ShippedOrders    = orders.Count(o => o.Status == OrderStatus.Shipped),
                DeliveredOrders  = orders.Count(o => o.Status == OrderStatus.Delivered),

                TotalRevenue     = revenue,
                BestSellingProducts = bestSellers,
                LatestOrders     = latestOrders
            };
        }
    }
}
