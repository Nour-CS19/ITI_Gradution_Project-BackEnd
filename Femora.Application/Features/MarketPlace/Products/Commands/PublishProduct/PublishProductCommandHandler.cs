using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Application.Features.Approvals.Common;
using Femora.Domain.Entities.Admin;
using Femora.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.MarketPlace.Products.Commands.PublishProduct
{
    public class PublishProductCommandHandler(IAppDbContext db, ICurrentUserService currentUser)
     : IRequestHandler<PublishProductCommand>
    {
        public async Task Handle(
            PublishProductCommand request,
            CancellationToken cancellationToken)
        {
            var product = await db.Products
                .Include(p => p.SellerProfile)
                .FirstOrDefaultAsync(
                    x => x.Id == request.ProductId,
                    cancellationToken);

            if (product is null)
                throw new NotFoundException("Product", request.ProductId.ToString());

            if (product.SellerProfile is null || product.SellerProfile.UserId != currentUser.UserId)
                throw new UnauthorizedAccessException("You don't own this product.");

            if (product.IsPuplished)
                throw new InvalidOperationException("This product is already live in the marketplace.");

            var hasPendingApproval = await db.ApprovalRequests.AnyAsync(
                x => x.EntityId == product.Id
                    && x.Type == ApprovalEntityType.ProductApproval
                    && x.ApprovalStatus == ApprovalStatus.Pending,
                cancellationToken);

            if (hasPendingApproval)
                throw new InvalidOperationException("This product is already awaiting admin review.");

            // Publishing = submitting for review. Product stays unpublished/invisible
            // until an admin approves the request (ReviewApprovalCommandHandler).
            db.ApprovalRequests.Add(new ApprovalRequest
            {
                Id = Guid.NewGuid(),
                RequsterId = product.SellerProfile.UserId,
                EntityId = product.Id,
                Type = ApprovalEntityType.ProductApproval,
                ApprovalStatus = ApprovalStatus.Pending,
                RequestedAt = DateTime.UtcNow,
                Note = new ApprovalNotePayload
                {
                    Title = product.Name,
                    Description = product.Description
                }.ToJson()
            });

            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
