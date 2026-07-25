using Femora.Domain.Entities.LMS;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Femora.Infrastructure.Data.Configurations.LMS;
public sealed class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> builder)
    {
        builder.ToTable("Lessons", schema: "lms");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.ModuleId).IsRequired();

        builder.Property(l => l.Title)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(l => l.Type)
               .IsRequired()
               .HasConversion<string>();

        builder.Property(l => l.ContentUrl)
               .HasMaxLength(500);

        builder.Property(l => l.OrderIndex)
               .IsRequired();

        builder.Property(l => l.IsPreview)
               .HasDefaultValue(false);

        builder.HasMany(l => l.Resources)
               .WithOne(r => r.Lesson)
               .HasForeignKey(r => r.LessonId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(l => l.LessonResources)
               .WithOne(r => r.Lesson)
               .HasForeignKey(r => r.LessonId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(l => l.LessonProgresses)
               .WithOne(lp => lp.Lesson)
               .HasForeignKey(lp => lp.LessonId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
