using Femora.Application.Common.Interfaces;
using Femora.Application.Features.LMS.Quizzes.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.LMS.Quizzes.Queries;

public class GetQuizAttemptByIdHandler : IRequestHandler<GetQuizAttemptByIdQuery, QuizAttemptDetailsDto>
{
    private readonly IAppDbContext _context;

    public GetQuizAttemptByIdHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<QuizAttemptDetailsDto> Handle(GetQuizAttemptByIdQuery request, CancellationToken cancellationToken)
    {
        var attempt = await _context.QuizAttempts
            .FirstOrDefaultAsync(x => x.Id == request.AttemptId, cancellationToken);

        if (attempt == null)
            throw new Exception("Quiz attempt not found");

        return new QuizAttemptDetailsDto
        {
            Id = attempt.Id,
            QuizId = attempt.QuizId,
            Score = attempt.Score,
            MaxScore = attempt.MaxScore,
            IsPassed = attempt.IsPassed,
            Answers = new List<QuizAttemptAnswerDto>()
        };
    }
}
