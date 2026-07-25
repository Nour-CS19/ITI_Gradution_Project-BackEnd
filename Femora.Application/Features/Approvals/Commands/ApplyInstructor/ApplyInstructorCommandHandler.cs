using Femora.Application.Common.Interfaces;
using Femora.Application.Features.Approvals.Common;
using Femora.Domain.Entities.Admin;
using Femora.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Femora.Application.Features.Approvals.Commands.ApplyInstructor;

public class ApplyInstructorCommandHandler : IRequestHandler<ApplyInstructorCommand, Guid>
{
    private readonly IAppDbContext _context;

    public ApplyInstructorCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(ApplyInstructorCommand request, CancellationToken cancellationToken)
    {
        // Prevent duplicate pending requests for the same entity and type
        var entityId = request.UserId; // using user id as the target entity for instructor verification
        var exists = await _context.ApprovalRequests
            .AnyAsync(x => x.EntityId == entityId && x.Type == ApprovalEntityType.InstructorVerification && x.ApprovalStatus == ApprovalStatus.Pending, cancellationToken);

        if (exists)
            throw new Femora.Application.Common.Exceptions.DuplicateApprovalRequestException("A pending instructor approval request already exists for this user.");

        var approval = new ApprovalRequest
        {
            Id = Guid.NewGuid(),
            RequsterId = request.UserId,
            EntityId = entityId,
            Type = ApprovalEntityType.InstructorVerification,
            ApprovalStatus = ApprovalStatus.Pending,
            RequestedAt = DateTime.UtcNow,
            Note = new ApprovalNotePayload
            {
                Bio = request.Bio,
                Portfolio = request.PortfolioUrl
            }.ToJson()
        };

        _context.ApprovalRequests.Add(approval);

        await _context.SaveChangesAsync(cancellationToken);

        return approval.Id;
    }
}