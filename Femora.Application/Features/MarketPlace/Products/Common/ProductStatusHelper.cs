using Femora.Domain.Enums;

namespace Femora.Application.Features.MarketPlace.Products.Common
{
    public static class ProductStatusHelper
    {
        /// <summary>
        /// Resolves the seller-facing ProductStatus from the product's publish flag
        /// and the status of its latest ProductApproval request (if any).
        /// No approval request at all => Draft (never submitted for review yet).
        /// </summary>
        public static ProductStatus Resolve(bool isPublished, ApprovalStatus? latestApprovalStatus)
        {
            if (isPublished)
                return ProductStatus.Approved;

            return latestApprovalStatus switch
            {
                null => ProductStatus.Draft,
                ApprovalStatus.Pending => ProductStatus.PendingApproval,
                ApprovalStatus.Approved => ProductStatus.Approved,
                ApprovalStatus.Rejected => ProductStatus.Rejected,
                _ => ProductStatus.Draft
            };
        }
    }

    public record ProductVariantInput(string Name, decimal Price, int StockQuantity, string? Color = null, string? Size = null, string? Material = null);
}
