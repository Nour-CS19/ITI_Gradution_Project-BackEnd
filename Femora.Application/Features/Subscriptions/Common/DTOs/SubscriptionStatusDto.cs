using Femora.Domain.Enums;

namespace Femora.Application.Features.Subscriptions.Common.DTOs;

public class SubscriptionStatusDto
{
    public Guid? SubscriptionId { get; set; }
    public Guid? PlanId { get; set; }
    public string? PlanName { get; set; }
    public BillingCycle BillingCycle { get; set; }
    public SubscriptionStatus Status { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}
