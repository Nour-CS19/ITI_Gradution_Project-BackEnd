using Femora.Domain.Entities.LMS;
using Femora.Domain.Entities;
using Femora.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Femora.Infrastructure.Data.Configurations.LMS
{
    public class CourseConfiguration : IEntityTypeConfiguration<Course>
    {
        public void Configure(EntityTypeBuilder<Course> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Title)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(c => c.Description)
                   .HasMaxLength(3000);

            builder.Property(c => c.ThumbnailUrl)
                   .HasMaxLength(500);

            builder.Property(c => c.Price)
                   .IsRequired()
                   .HasColumnType("decimal(18,2)");

            builder.Property(c => c.Category)
                   .IsRequired()
                   .HasMaxLength(100);

            // Store enum as string in DB
            builder.Property(c => c.Level)
                   .IsRequired()
                   .HasConversion<string>()
                   .HasMaxLength(20);

            builder.Property(c => c.Language)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(c => c.IsPublished)
                   .HasDefaultValue(false);

            builder.Property(c => c.RequiresApproval)
                   .HasDefaultValue(false);

            builder.Property(c => c.CreatedAt)
                   .HasDefaultValueSql("GETUTCDATE()");

            builder.HasMany(c => c.Modules)
                   .WithOne(m => m.Course)
                   .HasForeignKey(m => m.CourseId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.Enrollments)
                   .WithOne(e => e.Course)
                   .HasForeignKey(e => e.CourseId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.Assignments)
                   .WithOne(a => a.Course)
                   .HasForeignKey(a => a.CourseId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(c => c.Reviews)
                   .WithOne(r => r.Course)
                   .HasForeignKey(r => r.CourseId)
                   .OnDelete(DeleteBehavior.Cascade);

            // The course catalog (GetCoursesQueryHandler) always filters on IsPublished
            // first, then optionally narrows by Category / Level / Price, and sorts by
            // CreatedAt or Price. None of those columns had an index, so every catalog
            // request (search, filter, or plain browse) forced a full table scan. These
            // composite indexes cover the actual filter+sort combinations used today;
            // IsPublished leads each one since it's applied on every request.
            builder.HasIndex(c => new { c.IsPublished, c.CreatedAt })
                   .HasDatabaseName("IX_Courses_IsPublished_CreatedAt");

            builder.HasIndex(c => new { c.IsPublished, c.Category })
                   .HasDatabaseName("IX_Courses_IsPublished_Category");

            builder.HasIndex(c => new { c.IsPublished, c.Level })
                   .HasDatabaseName("IX_Courses_IsPublished_Level");

            builder.HasIndex(c => new { c.IsPublished, c.Price })
                   .HasDatabaseName("IX_Courses_IsPublished_Price");
        }
    }
}