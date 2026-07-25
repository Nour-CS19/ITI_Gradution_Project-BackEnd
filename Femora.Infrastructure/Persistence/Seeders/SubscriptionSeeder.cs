using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Femora.Domain.Entities;
using Femora.Domain.Entities.Identity;
using Femora.Domain.Entities.Subscription;
using Femora.Domain.Enums;
using Femora.Infrastructure.Data;

namespace Femora.Infrastructure.Persistence.Seeders
{
    internal static class SubscriptionSeeder
    {
        public static async Task SeedAsync(AppDbContext context, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            // Seed subscription plans
            await SeedSubscriptionPlansAsync(context);

            // Seed user subscriptions
            await SeedUserSubscriptionsAsync(context, userManager, configuration);
        }

        private static async Task SeedSubscriptionPlansAsync(AppDbContext context)
        {
            if (context.SubscriptionPlans.Any())
                return;

            var plans = new[]
            {
                new SubscriptionPlan
                {
                    Name = "Free",
                    Type = SubscriptionPlanType.Free,
                    MonthlyPrice = 0m,
                    YearlyPrice = 0m,
                    FeaturesJson = "[\"Access to free courses\", \"Community support\"]",
                    IsActive = true
                },
                new SubscriptionPlan
                {
                    Name = "Basic",
                    Type = SubscriptionPlanType.Basic,
                    MonthlyPrice = 9.99m,
                    YearlyPrice = 99.99m,
                    FeaturesJson = "[\"Access to all basic courses\", \"Email support\"]",
                    IsActive = true
                },
                new SubscriptionPlan
                {
                    Name = "Pro",
                    Type = SubscriptionPlanType.Pro,
                    MonthlyPrice = 19.99m,
                    YearlyPrice = 199.99m,
                    FeaturesJson = "[\"Access to pro courses\", \"Priority support\", \"Certificates\"]",
                    IsActive = true
                },
                new SubscriptionPlan
                {
                    Name = "Enterprise",
                    Type = SubscriptionPlanType.Enterprise,
                    MonthlyPrice = 99.99m,
                    YearlyPrice = 999.99m,
                    FeaturesJson = "[\"Team seats\", \"Dedicated support\", \"SLA\"]",
                    IsActive = true
                }
            };

            context.SubscriptionPlans.AddRange(plans);
            await context.SaveChangesAsync();
        }

        private static async Task SeedUserSubscriptionsAsync(AppDbContext context, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            var now = DateTime.UtcNow;

            // Get all plans
            var plans = await context.SubscriptionPlans
                .Where(p => p.IsActive)
                .OrderBy(p => p.Type)
                .ToListAsync();

            if (plans.Count == 0)
                return;

            // Define user email to plan type mapping
            var userPlanMapping = new Dictionary<string, SubscriptionPlanType>
            {
                { "admin@test.com", SubscriptionPlanType.Enterprise },
                { "trainee_1@test.com", SubscriptionPlanType.Basic },
                { "trainee_2@test.com", SubscriptionPlanType.Free },
                { "instructor_1@test.com", SubscriptionPlanType.Pro },
                { "instructor_2@test.com", SubscriptionPlanType.Pro },
                { "seller_1@test.com", SubscriptionPlanType.Basic },
                { "seller_2@test.com", SubscriptionPlanType.Free },
                { "multi_1@test.com", SubscriptionPlanType.Pro },
                { "multi_2@test.com", SubscriptionPlanType.Basic }
            };

            foreach (var userEmail in userPlanMapping.Keys)
            {
                var user = await userManager.FindByEmailAsync(userEmail);
                if (user == null)
                    continue;

                // Check if user already has a subscription
                if (context.UserSubscriptions.Any(s => s.UserId == user.Id))
                    continue;

                var planType = userPlanMapping[userEmail];
                var plan = plans.FirstOrDefault(p => p.Type == planType);
                if (plan == null)
                    continue;

                var billingCycle = (planType == SubscriptionPlanType.Enterprise) ? 
                    BillingCycle.Yearly : BillingCycle.Monthly;

                var endDate = billingCycle == BillingCycle.Yearly ? 
                    now.AddYears(1) : now.AddMonths(1);

                var subscription = new UserSubscription
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    SubscriptionPlanId = plan.Id,
                    BillingCycle = billingCycle,
                    Status = SubscriptionStatus.Active,
                    StartDate = now,
                    EndDate = endDate,
                    RenewedAt = now,
                    PaymentReference = $"SEED-{userEmail.Split('@')[0]}-{planType}"
                };
                context.UserSubscriptions.Add(subscription);
            }

            await context.SaveChangesAsync();
        }
    }
}
