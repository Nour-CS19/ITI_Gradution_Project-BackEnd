using Femora.Application.Common.Interfaces;
using Femora.Application.Features.Approvals.Common;
using Femora.Domain.Entities.Admin;
using Femora.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Femora.Application.Features.Approvals.Commands.ApplySeller;

public class ApplySellerCommandHandler : IRequestHandler<ApplySellerCommand, Guid>
{
    private readonly IAppDbContext _context;

    public ApplySellerCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(ApplySellerCommand request, CancellationToken cancellationToken)
    {
        var entityId = request.UserId; // use requester id as the entity for seller verification
        var exists = await _context.ApprovalRequests
            .AnyAsync(x => x.EntityId == entityId && x.Type == ApprovalEntityType.SellerVerification && x.ApprovalStatus == ApprovalStatus.Pending, cancellationToken);

        if (exists)
            throw new Femora.Application.Common.Exceptions.DuplicateApprovalRequestException("A pending seller approval request already exists for this user.");

        var approval = new ApprovalRequest
        {
            Id = Guid.NewGuid(),
            RequsterId = request.UserId,
            EntityId = entityId,
            Type = ApprovalEntityType.SellerVerification,
            ApprovalStatus =ApprovalStatus.Pending,
            RequestedAt = DateTime.UtcNow,

            // structured note
            Note = new ApprovalNotePayload
            {
                ShopName = request.ShopName,
                Description = request.Description
            }.ToJson()
        };

        _context.ApprovalRequests.Add(approval);

        await _context.SaveChangesAsync(cancellationToken);

        return approval.Id;
    }
}