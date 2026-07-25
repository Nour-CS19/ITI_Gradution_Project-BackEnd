using MediatR;
using Femora.Application.Features.Approvals.Common.DTOs;

namespace Femora.Application.Features.Approvals.Queries.GetPendingApprovals;

public class GetPendingApprovalsQuery : IRequest<List<ApprovalRequestDto>>
{
}
