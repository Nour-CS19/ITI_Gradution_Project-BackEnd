using MediatR;
using Femora.Domain.Enums;

namespace Femora.Application.Features.Subscriptions.Commands.UpgradeSubscription;

public class UpgradeSubscriptionCommand : IRequest<Guid>
{
    // Plan to subscribe to
    public Guid PlanId { get; set; }

    // Desired billing cycle
    public BillingCycle BillingCycle { get; set; } = BillingCycle.Monthly;

    // Filled by controller from authenticated user
    public Guid UserId { get; set; }
}
