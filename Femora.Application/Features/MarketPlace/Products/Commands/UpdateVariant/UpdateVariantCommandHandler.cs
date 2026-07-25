using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.MarketPlace.Products.Commands.UpdateVariant
{
    public class UpdateVariantCommandHandler(
        IAppDbContext db,
        ICurrentUserService currentUser)
     : IRequestHandler<UpdateVariantCommand>
    {
        public async Task Handle(
            UpdateVariantCommand request,
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

            variant.Name = request.Name;
            variant.Price = request.Price;
            variant.StockQuantity = request.StockQuantity;
            variant.Color = request.Color;
            variant.Size = request.Size;
            variant.Material = request.Material;

            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
