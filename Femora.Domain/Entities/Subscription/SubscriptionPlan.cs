using Femora.Domain.Common;
using Femora.Domain.Enums;

namespace Femora.Domain.Entities.Subscription;

public class SubscriptionPlan : BaseEntity
{
    public string Name { get; set; }
    public SubscriptionPlanType Type { get; set; }
    public decimal MonthlyPrice { get; set; }
    public decimal YearlyPrice { get; set; }
    public string FeaturesJson { get; set; }
    public bool IsActive { get; set; }
}
