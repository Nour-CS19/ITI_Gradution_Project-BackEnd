using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Femora.Domain.Entities.LMS.Quizzes;

namespace Femora.Infrastructure.Data.Configurations.LMS;

public sealed class QuizConfiguration : IEntityTypeConfiguration<Quiz>
{
    public void Configure(EntityTypeBuilder<Quiz> builder)
    {
        builder.ToTable("Quizzes", "lms");

        builder.HasKey(q => q.Id);
        builder.Property(q => q.Id).ValueGeneratedOnAdd();

        builder.Property(q => q.Title).IsRequired().HasMaxLength(200);

        builder.Property(q => q.MinimumPassingScore).HasDefaultValue(0);

        builder.Property(q => q.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

        builder.HasIndex(q => q.CourseId);
        builder.HasIndex(q => q.ModuleId);

        // Questions
        builder.HasMany(q => q.Questions)
               .WithOne(qst => qst.Quiz)
               .HasForeignKey(qst => qst.QuizId)
               .OnDelete(DeleteBehavior.Restrict);


        builder.HasMany(q => q.Attempts)
               .WithOne(a => a.Quiz)
               .HasForeignKey(a => a.QuizId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(q => q.Course)
               .WithMany()
               .HasForeignKey(q => q.CourseId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(q => q.Module)
       .WithOne(m => m.Quiz)
       .HasForeignKey<Quiz>(q => q.ModuleId);
    }
}
