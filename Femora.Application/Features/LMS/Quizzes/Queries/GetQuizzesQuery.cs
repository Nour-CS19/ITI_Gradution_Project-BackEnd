using MediatR;
using Femora.Application.Features.LMS.Quizzes.DTOs;

namespace Femora.Application.Features.LMS.Quizzes.Queries;

public record GetQuizzesQuery : IRequest<List<QuizSummaryDto>>;
