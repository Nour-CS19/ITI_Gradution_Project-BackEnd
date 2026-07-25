using Femora.Domain.Entities.Identity;
using Femora.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Femora.Infrastructure.Data.Configurations.Identity;
public class InstructorProfileConfiguration : IEntityTypeConfiguration<InstructorProfile>
{
    public void Configure(EntityTypeBuilder<InstructorProfile> builder)
    {
        builder.HasKey(ip => ip.Id);
        builder.HasIndex(ip => ip.UserId).IsUnique();

        // -- PROPERTIES ---
        builder.Property(ip => ip.Specialization)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(ip => ip.Bio)
             .HasMaxLength(1000);

        builder.Property(ip => ip.Rating)
            .HasColumnType("float")
            .HasDefaultValue(0.0);

        builder.Property(ip => ip.TotalEarnings)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        builder.Property(ip => ip.Status)
            .HasConversion<string>()
            .HasDefaultValue(VerificationStatus.Pending);

        builder.Property(ip => ip.VerifiedByAdminId)
            .IsRequired(false);

        builder.Property(ip => ip.VerifiedAt)
            .IsRequired(false);

        // -- RELATIONSHIPS --
        builder.HasOne(ip => ip.User)
            .WithOne(u => u.InstructorProfile)
            .HasForeignKey<InstructorProfile>(ip => ip.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(ip => ip.Courses)
            .WithOne(c => c.InstructorProfile)
            .HasForeignKey(c => c.InstructorProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(ip => ip.Credentials)
            .WithOne(c => c.InstructorProfile)
            .HasForeignKey(c => c.InstructorProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(ip => ip.Earnings)
            .WithOne(e => e.InstructorProfile)
            .HasForeignKey(e => e.InstructorProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
