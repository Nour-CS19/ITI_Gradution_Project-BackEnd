namespace Femora.API.Controllers.LMS.Requests;

using Femora.Application.Features.LMS.Quizzes.DTOs;

public class SubmitQuizRequest
{
    public Guid EnrollmentId { get; set; }
    public List<QuizAttemptAnswerDto> Answers { get; set; } = new();
}
