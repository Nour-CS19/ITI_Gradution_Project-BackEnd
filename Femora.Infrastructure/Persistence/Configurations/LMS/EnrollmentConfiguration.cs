using Femora.Domain.Entities.LMS;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Femora.Infrastructure.Data.Configurations.LMS;

public sealed class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("Enrollments", schema: "lms");

        // Primary key
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
               .ValueGeneratedNever();

        // Core properties
        builder.Property(e => e.TraineeProfileId)
               .IsRequired();

        builder.Property(e => e.CourseId)
               .IsRequired();

        builder.Property(e => e.EnrolledAt)
               .IsRequired()
               .HasDefaultValueSql("GETUTCDATE()");

        // Indexes
        builder.HasIndex(x => new
        {
            x.TraineeProfileId,
            x.CourseId
        }).IsUnique();

        builder.HasOne(e => e.Course)
               .WithMany(c => c.Enrollments)
               .HasForeignKey(e => e.CourseId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.TraineeProfile)
               .WithMany(t => t.Enrollments)
               .HasForeignKey(e => e.TraineeProfileId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
