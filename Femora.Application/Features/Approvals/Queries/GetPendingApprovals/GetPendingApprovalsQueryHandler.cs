using Femora.Application.Common.Interfaces;
using Femora.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Femora.Application.Features.Approvals.Common;
using Femora.Application.Features.Approvals.Common.DTOs;

namespace Femora.Application.Features.Approvals.Queries.GetPendingApprovals;

public class GetPendingApprovalsQueryHandler
    : IRequestHandler<GetPendingApprovalsQuery, List<ApprovalRequestDto>>
{
    private readonly IAppDbContext _context;

    public GetPendingApprovalsQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ApprovalRequestDto>> Handle(
        GetPendingApprovalsQuery request,
        CancellationToken cancellationToken)
    {
        // Pull the raw rows first (with the requester's identity info) then
        // parse the structured Note payload in memory, since Note parsing
        // is not translatable to SQL.
        var rows = await _context.ApprovalRequests
            .Where(x => x.ApprovalStatus == ApprovalStatus.Pending)
            .Join(
                _context.ApplicationUsers,
                approval => approval.RequsterId,
                user => user.Id,
                (approval, user) => new
                {
                    approval.Id,
                    approval.RequsterId,
                    approval.EntityId,
                    approval.Type,
                    approval.ApprovalStatus,
                    approval.RequestedAt,
                    approval.Note,
                    UserFullName = (user.FirstName + " " + user.LastName).Trim(),
                    user.Email
                })
            .ToListAsync(cancellationToken);

        return rows.Select(row =>
        {
            var payload = ApprovalNotePayload.Parse(row.Note);

            return new ApprovalRequestDto
            {
                Id = row.Id,
                UserId = row.RequsterId,
                RequesterName = row.UserFullName,
                UserFullName = row.UserFullName,
                UserEmail = row.Email,
                Type = row.Type,
                Status = row.ApprovalStatus,
                CreatedAt = row.RequestedAt,
                EntityId = row.EntityId,
                Bio = payload.Bio,
                PortfolioUrl = payload.Portfolio,
                ShopName = payload.ShopName,
                Description = payload.Description,
                Title = payload.Title
            };
        }).ToList();
    }
}