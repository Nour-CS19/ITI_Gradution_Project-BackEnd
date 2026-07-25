using Femora.Domain.Entities.LMS;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Femora.Infrastructure.Data.Configurations.LMS;

public class TraineeLearningGoalConfiguration : IEntityTypeConfiguration<TraineeLearningGoal>
{
    public void Configure(EntityTypeBuilder<TraineeLearningGoal> builder)
    {
        builder.ToTable("TraineeLearningGoals");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();

        builder.HasOne(x => x.TraineeProfile)
            .WithMany(t => t.LearningGoals)
            .HasForeignKey(x => x.TraineeProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Goal)
            .WithMany()
            .HasForeignKey(x => x.OnboardingGoalId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => new { x.TraineeProfileId, x.OnboardingGoalId })
            .IsUnique()
            .HasFilter("[OnboardingGoalId] IS NOT NULL");
    }
}
