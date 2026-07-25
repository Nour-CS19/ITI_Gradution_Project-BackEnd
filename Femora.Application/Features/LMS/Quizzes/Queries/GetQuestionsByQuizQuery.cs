using Femora.Application.Features.LMS.Quizzes.DTOs;
using MediatR;

namespace Femora.Application.Features.LMS.Quizzes.Queries;

public record GetQuestionsByQuizQuery(Guid QuizId) : IRequest<List<QuestionDto>>;