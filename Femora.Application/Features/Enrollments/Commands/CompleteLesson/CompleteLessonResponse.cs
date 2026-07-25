namespace Femora.Application.Features.Enrollments.Commands.CompleteLesson;

public class CompleteLessonResponse
{
    public Guid LessonId { get; init; }
    public Guid EnrollmentId { get; init; }
    public bool EnrollmentCompleted { get; init; }
    public DateTime? EnrollmentCompletedAt { get; init; }

    /// <summary>True when the completed lesson was the last (highest OrderIndex) lesson
    /// in its module - this is what the frontend uses to redirect straight into the quiz,
    /// with no manual "test me" button anywhere.</summary>
    public bool IsLastLessonInModule { get; init; }
    public Guid ModuleId { get; init; }

    /// <summary>Always populated when IsLastLessonInModule is true: if the module didn't
    /// have a quiz yet, one is generated automatically via the AI RAG pipeline right here.</summary>
    public Guid? ModuleQuizId { get; init; }
}
