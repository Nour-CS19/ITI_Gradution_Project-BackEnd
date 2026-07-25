using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.MarketPlace.Products.Commands.SetPrimaryImage
{
    public record SetPrimaryImageCommand(Guid ImageId) : IRequest;

    public class SetPrimaryImageCommandHandler(
        IAppDbContext db,
        ICurrentUserService currentUser)
     : IRequestHandler<SetPrimaryImageCommand>
    {
        public async Task Handle(SetPrimaryImageCommand request, CancellationToken cancellationToken)
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

            // Demote all others, then promote this one
            var allImages = await db.ProductImages
                .Where(i => i.ProductId == image.ProductId)
                .ToListAsync(cancellationToken);

            foreach (var img in allImages) img.IsPrimary = (img.Id == image.Id);

            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
