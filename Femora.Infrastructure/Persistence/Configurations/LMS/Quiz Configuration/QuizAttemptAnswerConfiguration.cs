

namespace Femora.Infrastructure.Persistence.Configurations.LMS.Quiz_Configuration
{
    using Femora.Domain.Entities.LMS;
    using Femora.Domain.Entities.LMS.Quizzes;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

    public class QuizAttemptAnswerConfiguration : IEntityTypeConfiguration<QuizAttemptAnswer>
    {
        public void Configure(EntityTypeBuilder<QuizAttemptAnswer> builder)
        {
            builder.HasKey(x => x.Id);

            // Relationship: Answer -> QuizAttempt
            builder.HasOne(x => x.QuizAttempt)
                   .WithMany(x => x.Answers)
                   .HasForeignKey(x => x.QuizAttemptId)
                   .OnDelete(DeleteBehavior.Cascade);

           
            builder.HasOne(x => x.Question)
                   .WithMany()
                   .HasForeignKey(x => x.QuestionId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Choice)
                   .WithMany()
                   .HasForeignKey(x => x.ChoiceId)
                   .OnDelete(DeleteBehavior.NoAction);

            builder.Property(x => x.IsCorrect)
                   .IsRequired();
        }
    }
}
