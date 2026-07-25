using Femora.Domain.Entities.LMS;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Infrastructure.Data.Configurations.LMS;
public class TraineePreferredCategoryConfiguration : IEntityTypeConfiguration<TraineePreferredCategory>
{
    public void Configure(EntityTypeBuilder<TraineePreferredCategory> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.TraineeProfileId, x.CourseCategoryId }).IsUnique();

        builder.HasOne(x => x.TraineeProfile)
            .WithMany(t => t.PreferredCategories)
            .HasForeignKey(x => x.TraineeProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

