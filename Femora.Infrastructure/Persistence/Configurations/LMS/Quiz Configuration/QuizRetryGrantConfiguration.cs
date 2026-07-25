using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Femora.Domain.Entities.LMS.Quizzes;

namespace Femora.Infrastructure.Persistence.Configurations.LMS.Quiz_Configuration
{
    public class QuizRetryGrantConfiguration : IEntityTypeConfiguration<QuizRetryGrant>
    {
        public void Configure(EntityTypeBuilder<QuizRetryGrant> builder)
        {
            builder.ToTable("QuizRetryGrants");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.GrantedAt).IsRequired();

            builder.HasOne(x => x.Quiz)
                .WithMany()
                .HasForeignKey(x => x.QuizId)
                .OnDelete(DeleteBehavior.Cascade);

            // A trainee can only ever have one grant per quiz - looked up together
            // constantly (SubmitQuizHandler + GetQuizWeakPointsHandler), so index it.
            builder.HasIndex(x => new { x.QuizId, x.EnrollmentId });
        }
    }
}
