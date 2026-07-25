using MediatR;
using Femora.Application.Features.LMS.Quizzes.DTOs;

namespace Femora.Application.Features.LMS.Quizzes.Commands;

public record SubmitQuizCommand(
    Guid QuizId,
    Guid EnrollmentId,
    List<QuizAttemptAnswerDto> Answers
) : IRequest<SubmitQuizResultDto>;