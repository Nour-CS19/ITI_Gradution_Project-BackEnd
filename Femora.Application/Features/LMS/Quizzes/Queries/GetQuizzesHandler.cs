using Femora.Application.Common.Interfaces;
using Femora.Application.Features.LMS.Quizzes.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.LMS.Quizzes.Queries;

public class GetQuizzesHandler : IRequestHandler<GetQuizzesQuery, List<QuizSummaryDto>>
{
    private readonly IAppDbContext _context;

    public GetQuizzesHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<QuizSummaryDto>> Handle(GetQuizzesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Quizzes
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
