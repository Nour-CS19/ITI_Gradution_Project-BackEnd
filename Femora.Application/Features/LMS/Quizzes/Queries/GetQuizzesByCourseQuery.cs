using Femora.Application.Features.LMS.Quizzes.DTOs;
using MediatR;

namespace Femora.Application.Features.LMS.Quizzes.Queries;

public record GetQuizzesByCourseQuery(Guid CourseId) : IRequest<List<QuizSummaryDto>>;