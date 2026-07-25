using Femora.Domain.Entities.Identity;
using Femora.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Femora.Infrastructure.Data.Configurations.Identity;
public class SellerProfileConfiguration : IEntityTypeConfiguration<SellerProfile>
{
    public void Configure(EntityTypeBuilder<SellerProfile> builder)
    {
        builder.HasKey(sp => sp.Id);
        builder.HasIndex(sp => sp.UserId).IsUnique();

        // -- PROPERTIES --
        builder.Property(sp => sp.StoreName)
         .HasMaxLength(200)
         .IsRequired();

        builder.Property(sp => sp.StoreDescription)
             .HasMaxLength(1000)
             .IsRequired(false);

        builder.Property(sp => sp.LogoUrl)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(sp => sp.CoverImageUrl)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(sp => sp.BusinessAddress)
            .HasMaxLength(300)
            .IsRequired(false);

        builder.Property(sp => sp.BusinessPhone)
            .HasMaxLength(30)
            .IsRequired(false);

        builder.Property(sp => sp.ContactEmail)
            .HasMaxLength(200)
            .IsRequired(false);

        builder.Property(sp => sp.TaxId)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(sp => sp.Rating)
            .HasColumnType("float")
            .HasDefaultValue(0.0);

        builder.Property(sp => sp.TotalEarnings)
            .HasColumnType("decimal(18,2)")
            .HasDefaultValue(0);

        builder.Property(sp => sp.TaxAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(sp => sp.Status)
            .HasConversion<string>()
            .HasDefaultValue(VerificationStatus.Pending);

        builder.Property(sp => sp.VerifiedByAdminId)
            .IsRequired(false);

        builder.Property(sp => sp.VerifiedAt)
            .IsRequired(false);


        // -- RELATIONSHIPS --
        builder.HasOne(sp => sp.User)
            .WithOne(u => u.SellerProfile)
            .HasForeignKey<SellerProfile>(sp => sp.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(sp => sp.Products)
               .WithOne(p => p.SellerProfile)
               .HasForeignKey(p => p.SellerProfileId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(sp => sp.Earnings)
               .WithOne(e => e.SellerProfile)
               .HasForeignKey(e => e.SellerProfileId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
