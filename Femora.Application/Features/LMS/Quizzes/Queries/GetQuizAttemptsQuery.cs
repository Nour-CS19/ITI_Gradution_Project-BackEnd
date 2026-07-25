using Femora.Application.Features.LMS.Quizzes.DTOs;
using MediatR;

namespace Femora.Application.Features.LMS.Quizzes.Queries;

public record GetQuizAttemptsQuery(Guid QuizId) : IRequest<List<QuizAttemptDto>>;