using Femora.Domain.Entities.LMS;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Femora.Infrastructure.Data.Configurations.LMS;

public sealed class LessonResourceConfiguration : IEntityTypeConfiguration<LessonResource>
{
    public void Configure(EntityTypeBuilder<LessonResource> builder)
    {
        builder.ToTable("LessonResources", schema: "lms");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.LessonId).IsRequired();

        builder.Property(r => r.FileName)
               .IsRequired()
               .HasMaxLength(255);

        builder.Property(r => r.BlobUrl)
               .IsRequired()
               .HasMaxLength(500);

        builder.Property(r => r.ContentType)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(r => r.Status)
               .IsRequired()
               .HasConversion<string>();

        builder.Property(r => r.ChunkCount)
               .HasDefaultValue(0);

        builder.Property(r => r.ErrorMessage)
               .HasMaxLength(2000);

        builder.Property(r => r.UploadedAt)
               .IsRequired();

        builder.HasOne(r => r.Lesson)
               .WithMany(l => l.LessonResources)
               .HasForeignKey(r => r.LessonId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
