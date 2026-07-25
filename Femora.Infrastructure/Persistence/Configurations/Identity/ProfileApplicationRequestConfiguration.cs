using Femora.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Femora.Infrastructure.Data.Configurations.Identity;

public class ProfileApplicationRequestConfiguration : IEntityTypeConfiguration<ProfileApplicationRequest>
{
    public void Configure(EntityTypeBuilder<ProfileApplicationRequest> builder)
    {
        builder.ToTable("ProfileApplicationRequests");

        builder.HasKey(r => r.Id);

        // Properties
        builder.Property(r => r.Bio)
            .HasMaxLength(1000);

        builder.Property(r => r.PortfolioUrl)
            .HasMaxLength(500);

        builder.Property(r => r.NationalIdNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.StoreName)
            .HasMaxLength(200);

        builder.Property(r => r.StoreDescription)
            .HasMaxLength(1000);

        builder.Property(r => r.RejectionReason)
            .HasMaxLength(500);

        // Unique index for active pending requests (UserId, RequestedRole) when Status is Pending (0)
        builder.HasIndex(r => new { r.UserId, r.RequestedRole, r.Status })
            .HasDatabaseName("IX_Unique_Pending_Application")
            .HasFilter("[Status] = 0")
            .IsUnique();

        // Relationships
        builder.HasOne(r => r.User)
            .WithMany(u => u.ProfileApplicationRequests)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.ReviewedByAdmin)
            .WithMany()
            .HasForeignKey(r => r.ReviewedByAdminId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
