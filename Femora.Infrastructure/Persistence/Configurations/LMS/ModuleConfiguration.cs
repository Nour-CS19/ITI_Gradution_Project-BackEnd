using Femora.Domain.Entities.LMS;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Femora.Infrastructure.Data.Configurations.LMS;
public sealed class ModuleConfiguration : IEntityTypeConfiguration<Module>
{
    public void Configure(EntityTypeBuilder<Module> builder)
    {
        builder.ToTable("Modules", schema: "lms");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.CourseId).IsRequired();

        builder.Property(m => m.Title)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(m => m.OrderIndex)
               .IsRequired();

        builder.HasMany(m => m.Lessons)
               .WithOne(l => l.Module)
               .HasForeignKey(l => l.ModuleId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
