using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Application.Features.MarketPlace.Products.Commands.ApproveProduct
{
    public class ApproveProductCommandHandler(IAppDbContext db)
     : IRequestHandler<ApproveProductCommand>
    {
        public async Task Handle(
            ApproveProductCommand request,
            CancellationToken cancellationToken)
        {
            var product = await db.Products
                .FirstOrDefaultAsync(
                    x => x.Id == request.ProductId,
                    cancellationToken);

            if (product is null)
            {
                throw new NotFoundException(
                    "Product",
                    request.ProductId.ToString());
            }

            product.IsPuplished = true;

            var approvalRequest = await db.ApprovalRequests
                .Where(x => x.EntityId == request.ProductId
                    && x.Type == ApprovalEntityType.ProductApproval
                    && x.ApprovalStatus == ApprovalStatus.Pending)
                .OrderByDescending(x => x.RequestedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (approvalRequest is not null)
            {
                approvalRequest.ApprovalStatus = ApprovalStatus.Approved;
                approvalRequest.AdminId = request.AdminId;
                approvalRequest.ReviewedAt = DateTime.UtcNow;
            }

            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
