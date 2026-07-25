using Femora.Domain.Common;

using Femora.Domain.Enums;


namespace Femora.Domain.Entities.Subscription;



public class UserSubscription : BaseEntity

{
    public Guid UserId { get; set; }
    public Guid SubscriptionPlanId { get; set; }
    public BillingCycle BillingCycle { get; set; }
    public SubscriptionStatus Status { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; }
    public DateTime RenewedAt { get; set; }
    public string PaymentReference { get; set; }
    public ApplicationUser User { get; set; }
    public SubscriptionPlan SubscriptionPlan { get; set; }
}
