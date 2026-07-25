using MediatR;
using Femora.Application.Features.LMS.Quizzes.DTOs;

namespace Femora.Application.Features.LMS.Quizzes.Queries;

public record GetQuizAttemptByIdQuery(Guid AttemptId) : IRequest<QuizAttemptDetailsDto>;
