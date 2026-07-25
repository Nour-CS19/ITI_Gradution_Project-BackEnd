using Femora.Application.Common.Interfaces;
using Femora.Application.Features.LMS.Quizzes.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.LMS.Quizzes.Queries;

public class GetQuizAttemptsHandler : IRequestHandler<GetQuizAttemptsQuery, List<QuizAttemptDto>>
{
    private readonly IAppDbContext _context;

    public GetQuizAttemptsHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<QuizAttemptDto>> Handle(GetQuizAttemptsQuery request, CancellationToken cancellationToken)
    {
        return await _context.QuizAttempts
            .Where(x => x.QuizId == request.QuizId)
            .Select(x => new QuizAttemptDto
            {
                Id = x.Id,
                QuizId = x.QuizId,
                Score = x.Score,
                IsPassed = x.IsPassed,
                AttemptedAt = x.AttemptedAt
            })
            .ToListAsync(cancellationToken);
    }
}
