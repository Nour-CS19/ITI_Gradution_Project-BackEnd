using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Infrastructure.Persistence.Configurations.LMS.Quiz_Configuration
{
   
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;
    using Femora.Domain.Entities.LMS;
    using Femora.Domain.Entities.LMS.Quizzes;

    public class QuizAttemptConfiguration : IEntityTypeConfiguration<QuizAttempt>
    {
        public void Configure(EntityTypeBuilder<QuizAttempt> builder)
        {
            builder.ToTable("QuizAttempts");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.AttemptedAt)
                .IsRequired();

            builder.Property(x => x.SubmittedAt)
                .IsRequired(false);

            builder.Property(x => x.Score)
                .HasPrecision(5, 2);

            builder.Property(x => x.Percentage)
                .HasPrecision(5, 2);

            // Relationship: QuizAttempt -> Quiz
            builder.HasOne(x => x.Quiz)
                .WithMany(x => x.Attempts)
                .HasForeignKey(x => x.QuizId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relationship: QuizAttempt -> User
            builder.HasOne(x => x.TraineeProfile)
                .WithMany()
                .HasForeignKey(x => x.TraineeProfileId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relationship: QuizAttempt -> Answers
            builder.HasMany(x => x.Answers)
                .WithOne(x => x.QuizAttempt)
                .HasForeignKey(x => x.QuizAttemptId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
