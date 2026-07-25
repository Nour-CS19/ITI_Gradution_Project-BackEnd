using Femora.Domain.Entities;
using Femora.Domain.Entities.Onboarding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Femora.Infrastructure.Data.Configurations.Identity;
public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.HasKey(u => u.Id);

        // -- PROPERTIES ---
        builder.Property(u => u.FirstName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.LastName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.AvatarUrl)
            .HasMaxLength(500);

        builder.Property(u => u.Bio)
            .HasMaxLength(1000);

        builder.Property(u => u.LinkedInUrl)
            .HasMaxLength(300);

        builder.Property(u => u.GitHubUrl)
            .HasMaxLength(300);

        builder.Property(u => u.Country)
            .HasMaxLength(100);

        builder.Property(u => u.IsActive)
            .HasDefaultValue(true);

        builder.Property(u => u.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasMany(u => u.OnboardingInterests)
            .WithMany()
            .UsingEntity<Dictionary<string, object>>(
                "UserInterests",
                j => j.HasOne<OnboardingInterest>().WithMany().HasForeignKey("OnboardingInterestId").OnDelete(DeleteBehavior.Cascade),
                j => j.HasOne<ApplicationUser>().WithMany().HasForeignKey("ApplicationUserId").OnDelete(DeleteBehavior.Cascade)
            );

        builder.HasOne(u => u.OnboardingGoal)
            .WithMany()
            .HasForeignKey(u => u.OnboardingGoalId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(u => u.Orders)
            .WithOne(u => u.User)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
