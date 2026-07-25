using Femora.Domain.Entities.LMS;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Femora.Infrastructure.Persistence.Configurations.LMS;
public class EnrollmentModuleConfigruation : IEntityTypeConfiguration<EnrollmentModule>
{
    public void Configure(EntityTypeBuilder<EnrollmentModule> builder)
    {
        builder.HasKey(x => new
        {
            x.EnrollmentId,
            x.ModuleId
        });

        builder.HasIndex(x => new { x.EnrollmentId, x.ModuleId }).IsUnique();

        builder.Property(x => x.IsUnlocked)
            .HasDefaultValue(false);

        builder.HasOne(x => x.Enrollment)
            .WithMany(e => e.EnrollmentModules)
            .HasForeignKey(x => x.EnrollmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Module)
            .WithMany(m => m.EnrollmentModules)
            .HasForeignKey(x => x.ModuleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
