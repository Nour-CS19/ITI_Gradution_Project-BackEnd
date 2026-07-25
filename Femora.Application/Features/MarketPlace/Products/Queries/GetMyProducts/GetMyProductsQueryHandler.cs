using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Application.Common.Models;
using Femora.Application.Features.Approvals.Common;
using Femora.Application.Features.MarketPlace.Products.Common;
using Femora.Application.Features.MarketPlace.Products.DTOs;
using Femora.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.MarketPlace.Products.Queries.GetMyProducts
{
    public class GetMyProductsQueryHandler(
        IAppDbContext db,
        ICurrentUserService currentUser)
        : IRequestHandler<GetMyProductsQuery, PagedResult<MyProductSummaryDto>>
    {
        public async Task<PagedResult<MyProductSummaryDto>> Handle(
            GetMyProductsQuery request,
            CancellationToken cancellationToken)
        {
            var sellerProfileId = await db.SellerProfiles
                .AsNoTracking()
                .Where(sp => sp.UserId == currentUser.UserId)
                .Select(sp => (Guid?)sp.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (sellerProfileId is null)
                throw new NotFoundException("SellerProfile", currentUser.UserId.ToString());

            var query = db.Products
                .AsNoTracking()
                .Where(p => p.SellerProfileId == sellerProfileId.Value);

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                query = query.Where(p => p.Name.Contains(request.Search));
            }

            if (request.CategoryId.HasValue)
            {
                query = query.Where(p => p.ProductCategoryId == request.CategoryId.Value);
            }

            var all = await query
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Description,
                    p.IsPuplished,
                    p.ProductCategoryId,
                    CategoryName = p.ProductCategory != null ? p.ProductCategory.Name : null,
                    ImageUrl = p.ProductImages
                        .Where(i => i.IsPrimary)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault(),
                    ImageUrls = p.ProductImages
                        .OrderBy(i => i.OrderIndex)
                        .Select(i => i.ImageUrl)
                        .Take(3)
                        .ToList(),
                    MinPrice = p.ProductVariants.Any() ? p.ProductVariants.Min(v => v.Price) : 0,
                    TotalStock = p.ProductVariants.Sum(v => (int?)v.StockQuantity) ?? 0
                })
                .ToListAsync(cancellationToken);

            var productIds = all.Select(i => i.Id).ToList();

            var latestApprovals = await db.ApprovalRequests
                .AsNoTracking()
                .Where(a => a.Type == ApprovalEntityType.ProductApproval && productIds.Contains(a.EntityId))
                .OrderByDescending(a => a.RequestedAt)
                .GroupBy(a => a.EntityId)
                .Select(g => new { ProductId = g.Key, Latest = g.First() })
                .ToDictionaryAsync(x => x.ProductId, x => x.Latest, cancellationToken);

            var dtoItems = all.Select(p =>
            {
                latestApprovals.TryGetValue(p.Id, out var latest);
                var status = ProductStatusHelper.Resolve(p.IsPuplished, latest?.ApprovalStatus);
                var adminNote = status == ProductStatus.Rejected
                    ? ApprovalNotePayload.Parse(latest?.Note).AdminNote
                    : null;

                return new MyProductSummaryDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    ImageUrl = p.ImageUrl,
                    ImageUrls = p.ImageUrls,
                    MinPrice = p.MinPrice,
                    TotalStock = p.TotalStock,
                    CategoryId = p.ProductCategoryId,
                    CategoryName = p.CategoryName,
                    IsPublished = p.IsPuplished,
                    Status = status.ToString(),
                    AdminNote = adminNote
                };
            }).ToList();

            if (!string.IsNullOrWhiteSpace(request.Status)
                && Enum.TryParse<ProductStatus>(request.Status, true, out var statusFilter))
            {
                dtoItems = dtoItems.Where(d => d.Status == statusFilter.ToString()).ToList();
            }

            var total = dtoItems.Count;

            var pageItems = dtoItems
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new PagedResult<MyProductSummaryDto>
            {
                Items = pageItems,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalCount = total
            };
        }
    }
}
