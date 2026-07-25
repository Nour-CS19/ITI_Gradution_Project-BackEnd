using Femora.Domain.Entities.Marketplace;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Femora.Infrastructure.Persistence.Configurations.Marketplace;

public class TraineePreferredProductCategoryConfiguration : IEntityTypeConfiguration<TraineePreferredProductCategory>
{
    public void Configure(EntityTypeBuilder<TraineePreferredProductCategory> builder)
    {
        builder.ToTable("TraineePreferredProductCategories");

        builder.HasKey(t => t.Id);

        builder.HasOne(t => t.TraineeProfile)
            .WithMany()
            .HasForeignKey(t => t.TraineeProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(t => t.ProductCategory)
            .WithMany()
            .HasForeignKey(t => t.ProductCategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // A trainee shouldn't have the same product category twice
        builder.HasIndex(t => new { t.TraineeProfileId, t.ProductCategoryId }).IsUnique();
    }
}
