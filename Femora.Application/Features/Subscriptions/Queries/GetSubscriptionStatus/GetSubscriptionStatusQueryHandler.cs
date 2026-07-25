using System.Threading;
using System.Threading.Tasks;
using Femora.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Femora.Application.Features.Subscriptions.Common.DTOs;

namespace Femora.Application.Features.Subscriptions.Queries.GetSubscriptionStatus;

public class GetSubscriptionStatusQueryHandler : IRequestHandler<GetSubscriptionStatusQuery, SubscriptionStatusDto?>
{
    private readonly IAppDbContext _context;

    public GetSubscriptionStatusQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<SubscriptionStatusDto?> Handle(GetSubscriptionStatusQuery request, CancellationToken cancellationToken)
    {
        var sub = await _context.UserSubscriptions
            .Include(s => s.SubscriptionPlan)
            .Where(s => s.UserId == request.UserId)
            .OrderByDescending(s => s.StartDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (sub == null)
            return null;

        return new SubscriptionStatusDto
        {
            SubscriptionId = sub.Id,
            PlanId = sub.SubscriptionPlanId,
            PlanName = sub.SubscriptionPlan?.Name,
            BillingCycle = sub.BillingCycle,
            Status = sub.Status,
            StartDate = sub.StartDate,
            EndDate = sub.EndDate
        };
    }
}
