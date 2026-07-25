using Femora.Application.Features.LMS.Quizzes.DTOs;
using MediatR;

namespace Femora.Application.Features.LMS.Quizzes.Queries;

/// <summary>
/// Generates an AI-grounded review of the mistakes on a trainee's last attempt of a
/// quiz they've now exhausted all regular attempts on, and - the first time it's
/// requested - unlocks exactly one bonus attempt (see QuizRetryGrant).
/// </summary>
public class GetQuizWeakPointsQuery : IRequest<QuizWeakPointsReportDto>
{
    public Guid QuizId { get; set; }
    public Guid EnrollmentId { get; set; }
    public Guid UserId { get; set; }
}
