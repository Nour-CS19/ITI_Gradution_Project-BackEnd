using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Infrastructure.Persistence.Configurations.LMS.Quiz_Configuration
{
    /* internal class LessonProgressConfiguration
     {
     }*/
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Femora.Domain.Entities.LMS;

    public class LessonProgressConfiguration : IEntityTypeConfiguration<LessonProgress>
    {
        public void Configure(EntityTypeBuilder<LessonProgress> builder)
        {
            builder.HasKey(x => x.Id);

            // =========================
            // العلاقة مع Enrollment (CASCADE مسموح)
            // =========================
            builder.HasOne(x => x.Enrollment)
                   .WithMany(e => e.LessonProgresses)
                   .HasForeignKey(x => x.EnrollmentId)
                   .OnDelete(DeleteBehavior.Cascade);

            // =========================
            // العلاقة مع Lesson (مهم جداً: منع CASCADE)
            // =========================
            builder.HasOne(x => x.Lesson)
                   .WithMany(l => l.LessonProgresses)
                   .HasForeignKey(x => x.LessonId)
                   .OnDelete(DeleteBehavior.NoAction);

            // =========================
            // الخصائص
            // =========================
            builder.Property(x => x.IsCompleted)
                   .IsRequired();

            builder.Property(x => x.WatchedSeconds)
                   .IsRequired(false);

            builder.Property(x => x.LastAccessedAt)
                   .IsRequired(false);
        }
    }
}
