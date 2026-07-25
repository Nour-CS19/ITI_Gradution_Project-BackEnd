using MediatR;

namespace Femora.Application.Features.Approvals.Commands.ReviewApproval;

public class ReviewApprovalCommand : IRequest<bool>
{
    public Guid ApprovalId { get; set; }
    public Guid AdminId { get; set; }
    public bool IsApproved { get; set; }
    public string? Note { get; set; }
}