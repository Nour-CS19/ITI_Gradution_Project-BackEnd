using System;
using System.Threading;
using System.Threading.Tasks;
using Femora.Application.Common.Interfaces;
using Femora.Domain.Entities.Subscription;
using Femora.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.Subscriptions.Commands.UpgradeSubscription;

public class UpgradeSubscriptionCommandHandler : IRequestHandler<UpgradeSubscriptionCommand, Guid>
{
    private readonly IAppDbContext _context;

    public UpgradeSubscriptionCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(UpgradeSubscriptionCommand request, CancellationToken cancellationToken)
    {
        // Validate plan
        var plan = await _context.SubscriptionPlans
            .FirstOrDefaultAsync(p => p.Id == request.PlanId && p.IsActive, cancellationToken);

        if (plan == null)
            throw new ArgumentException("Subscription plan not found or inactive.");

        // Calculate dates
        var now = DateTime.UtcNow;
        DateTime endDate = request.BillingCycle == BillingCycle.Monthly
            ? now.AddMonths(1)
            : now.AddYears(1);

        // Deactivate existing active subscription (if any)
        var current = await _context.UserSubscriptions
            .Where(s => s.UserId == request.UserId && s.Status == SubscriptionStatus.Active)
            .ToListAsync(cancellationToken);

        foreach (var sub in current)
        {
            sub.Status = SubscriptionStatus.Expired;
            sub.EndDate = now;
        }

        var userSubscription = new UserSubscription
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            SubscriptionPlanId = plan.Id,
            BillingCycle = request.BillingCycle,
            Status = SubscriptionStatus.Active,
            StartDate = now,
            EndDate = endDate,
            RenewedAt = now,
            PaymentReference = null
        };

        _context.UserSubscriptions.Add(userSubscription);
        await _context.SaveChangesAsync(cancellationToken);

        return userSubscription.Id;
    }
}
