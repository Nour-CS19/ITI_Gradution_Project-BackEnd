using Femora.Domain.Entities.Identity;
using Femora.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Femora.Infrastructure.Data.Configurations.Identity;
public class TraineeProfileConfiguration : IEntityTypeConfiguration<TraineeProfile>
{
    public void Configure(EntityTypeBuilder<TraineeProfile> builder)
    {

        builder.HasKey(tp => tp.Id);
        builder.HasIndex(tp => tp.UserId).IsUnique();

        // -- PROPERTIES --
        builder.Property(tp => tp.SkillLevel)
            .HasConversion<string>()
            .HasDefaultValue(TrainingSkillLevel.Beginner);

        // -- RELATIONSHIPS --
        builder.HasOne(tp => tp.User)
            .WithOne(u => u.TraineeProfile)
            .HasForeignKey<TraineeProfile>(tp => tp.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(tp => tp.LearningGoals)
            .WithOne(lg => lg.TraineeProfile)
            .HasForeignKey(lg => lg.TraineeProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(tp => tp.PreferredCategories)
            .WithOne(pc => pc.TraineeProfile)
            .HasForeignKey(pc => pc.TraineeProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
