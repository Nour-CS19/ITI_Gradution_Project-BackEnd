using Femora.Domain.Entities.AI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Infrastructure.Data.Configurations.Ai;

public class AIRecommendationConfiguration : IEntityTypeConfiguration<AIRecommendation>
{
    public void Configure(EntityTypeBuilder<AIRecommendation> builder)
    {
        builder.ToTable("AIRecommendations");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(r => r.EntityId)
            .IsRequired();

        builder.Property(r => r.EntityType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(r => r.IsViewed)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(r => r.GeneratedAt)
            .IsRequired();

        builder.Property(r => r.ReasonJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(r => r.Score)
            .IsRequired()
            .HasColumnType("float");

        // One ApplicationUser has many AIRecommendations
        builder.HasOne(r => r.User)
            .WithMany(u => u.AIRecommendations)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => new { r.EntityType, r.EntityId });
    }
}