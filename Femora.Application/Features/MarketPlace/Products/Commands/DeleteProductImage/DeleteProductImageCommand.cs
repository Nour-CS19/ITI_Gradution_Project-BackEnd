using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.MarketPlace.Products.Commands.DeleteProductImage
{
    public record DeleteProductImageCommand(Guid ImageId) : IRequest;

    public class DeleteProductImageCommandHandler(
        IAppDbContext db,
        ICurrentUserService currentUser)
     : IRequestHandler<DeleteProductImageCommand>
    {
        public async Task Handle(DeleteProductImageCommand request, CancellationToken cancellationToken)
        {
            var image = await db.ProductImages
                .Include(i => i.Product)
                    .ThenInclude(p => p!.SellerProfile)
                .FirstOrDefaultAsync(i => i.Id == request.ImageId, cancellationToken);

            if (image is null)
                throw new NotFoundException("ProductImage", request.ImageId.ToString());

            if (image.Product?.SellerProfile?.UserId != currentUser.UserId)
                throw new UnauthorizedAccessException("You don't own this product.");

            var hasPendingApproval = await db.ApprovalRequests.AnyAsync(
                x => x.EntityId == image.ProductId
                    && x.Type == ApprovalEntityType.ProductApproval
                    && x.ApprovalStatus == ApprovalStatus.Pending,
                cancellationToken);

            if (hasPendingApproval)
                throw new InvalidOperationException("Product is under review and cannot be edited.");

            // Ensure at least one image remains
            var count = await db.ProductImages.CountAsync(i => i.ProductId == image.ProductId, cancellationToken);
            if (count <= 1)
                throw new InvalidOperationException("A product must have at least one image.");

            // If we're removing the primary, promote the next by OrderIndex
            if (image.IsPrimary)
            {
                var next = await db.ProductImages
                    .Where(i => i.ProductId == image.ProductId && i.Id != image.Id)
                    .OrderBy(i => i.OrderIndex)
                    .FirstOrDefaultAsync(cancellationToken);

                if (next is not null) next.IsPrimary = true;
            }

            db.ProductImages.Remove(image);
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
