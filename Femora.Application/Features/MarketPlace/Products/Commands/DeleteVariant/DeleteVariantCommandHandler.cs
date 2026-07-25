using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.MarketPlace.Products.Commands.DeleteVariant
{
    public class DeleteVariantCommandHandler(
        IAppDbContext db,
        ICurrentUserService currentUser)
     : IRequestHandler<DeleteVariantCommand>
    {
        public async Task Handle(
            DeleteVariantCommand request,
            CancellationToken cancellationToken)
        {
            var variant = await db.ProductVariants
                .Include(v => v.Product)
                    .ThenInclude(p => p!.SellerProfile)
                .FirstOrDefaultAsync(v => v.Id == request.VariantId, cancellationToken);

            if (variant is null)
                throw new NotFoundException("ProductVariant", request.VariantId.ToString());

            if (variant.Product is null
                || variant.Product.SellerProfile is null
                || variant.Product.SellerProfile.UserId != currentUser.UserId)
                throw new UnauthorizedAccessException("You don't own this product.");

            var hasPendingApproval = await db.ApprovalRequests.AnyAsync(
                x => x.EntityId == variant.ProductId
                    && x.Type == ApprovalEntityType.ProductApproval
                    && x.ApprovalStatus == ApprovalStatus.Pending,
                cancellationToken);

            if (hasPendingApproval)
                throw new InvalidOperationException(
                    "This product is awaiting admin review and can't be edited right now.");

            // Prevent leaving the product with zero variants
            var variantCount = await db.ProductVariants
                .CountAsync(v => v.ProductId == variant.ProductId, cancellationToken);

            if (variantCount <= 1)
                throw new InvalidOperationException(
                    "A product must have at least one variant. Add another variant before removing this one.");

            db.ProductVariants.Remove(variant);
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
