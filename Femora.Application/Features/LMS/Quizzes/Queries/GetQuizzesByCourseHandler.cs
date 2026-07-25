using Femora.Application.Common.Interfaces;
using Femora.Application.Features.LMS.Quizzes.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.LMS.Quizzes.Queries;

public class GetQuizzesByCourseHandler : IRequestHandler<GetQuizzesByCourseQuery, List<QuizSummaryDto>>
{
    private readonly IAppDbContext _context;

    public GetQuizzesByCourseHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<QuizSummaryDto>> Handle(GetQuizzesByCourseQuery request, CancellationToken cancellationToken)
    {
        return await _context.Quizzes
            .Where(q => q.CourseId == request.CourseId)
            .Select(q => new QuizSummaryDto
            {
                Id = q.Id,
                Title = q.Title,
                MaxAttempts = q.MaxAttempts,
                QuestionsCount = q.Questions.Count
            })
            .ToListAsync(cancellationToken);
    }
}