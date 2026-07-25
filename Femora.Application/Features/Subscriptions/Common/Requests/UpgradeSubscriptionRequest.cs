using Femora.Domain.Enums;

namespace Femora.Application.Features.Subscriptions.Common.Requests;

public record UpgradeSubscriptionRequest
{
    public Guid PlanId { get; set; }
    public BillingCycle BillingCycle { get; set; } = BillingCycle.Monthly;
}
