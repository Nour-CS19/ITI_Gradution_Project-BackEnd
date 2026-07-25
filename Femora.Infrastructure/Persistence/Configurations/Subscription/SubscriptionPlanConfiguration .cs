using Femora.Domain.Entities.Subscription;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Infrastructure.Persistence.Configurations.Subscription;

public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.MonthlyPrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.YearlyPrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.Type)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.FeaturesJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.IsActive)
            .HasDefaultValue(true);
    }
}
