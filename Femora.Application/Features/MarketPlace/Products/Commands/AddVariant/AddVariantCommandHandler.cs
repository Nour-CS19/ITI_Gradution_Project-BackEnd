using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Domain.Entities.Marketplace;
using Femora.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.MarketPlace.Products.Commands.AddVariant
{
    public class AddVariantCommandHandler(
        IAppDbContext db,
        ICurrentUserService currentUser)
     : IRequestHandler<AddVariantCommand, Guid>
    {
        public async Task<Guid> Handle(
            AddVariantCommand request,
            CancellationToken cancellationToken)
        {
            var product = await db.Products
                .Include(p => p.SellerProfile)
                .FirstOrDefaultAsync(x => x.Id == request.ProductId, cancellationToken);

            if (product is null)
                throw new NotFoundException("Product", request.ProductId.ToString());

            if (product.SellerProfile is null || product.SellerProfile.UserId != currentUser.UserId)
                throw new UnauthorizedAccessException("You don't own this product.");

            // Block if a pending approval request exists
            var hasPendingApproval = await db.ApprovalRequests.AnyAsync(
                x => x.EntityId == product.Id
                    && x.Type == ApprovalEntityType.ProductApproval
                    && x.ApprovalStatus == ApprovalStatus.Pending,
                cancellationToken);

            if (hasPendingApproval)
                throw new InvalidOperationException(
                    "This product is awaiting admin review and can't be edited right now.");

            var variant = new ProductVariant
            {
                ProductId = product.Id,
                Name = request.Name,
                Price = request.Price,
                StockQuantity = request.StockQuantity,
                Color = request.Color,
                Size = request.Size,
                Material = request.Material
            };

            db.ProductVariants.Add(variant);
            await db.SaveChangesAsync(cancellationToken);

            return variant.Id;
        }
    }
}
