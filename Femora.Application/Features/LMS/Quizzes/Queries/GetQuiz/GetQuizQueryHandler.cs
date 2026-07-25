using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Femora.Application.Features.LMS.Quizzes.Queries.GetQuiz;

public class GetQuizQueryHandler(IAppDbContext db) : IRequestHandler<GetQuizQuery, GetQuizResponse>
{
    public async Task<GetQuizResponse> Handle(GetQuizQuery request, CancellationToken cancellationToken)
    {
        var quiz = await db.Quizzes
            .Include(q => q.Questions)
                .ThenInclude(qst => qst.Choices)
            .FirstOrDefaultAsync(q => q.Id == request.QuizId, cancellationToken)
            ?? throw new NotFoundException("Quiz", request.QuizId.ToString());

        return new GetQuizResponse
        {
            QuizId = quiz.Id,
            Title = quiz.Title,
            CourseId = quiz.CourseId,
            ModuleId = quiz.ModuleId,
            MinimumPassingScore = quiz.MinimumPassingScore,
            MaxAttempts = quiz.MaxAttempts,
            Questions = quiz.Questions
                .OrderBy(q => q.OrderIndex)
                .Select(q => new QuizQuestionDto
                {
                    QuestionId = q.Id,
                    Text = q.Text,
                    Type = q.Type.ToString(),
                    OrderIndex = q.OrderIndex,
                    Choices = q.Choices
                        .OrderBy(c => c.Order)
                        .Select(c => new QuizChoiceDto
                        {
                            ChoiceId = c.Id,
                            Text = c.Text,
                            Order = c.Order
                        }).ToList()
                }).ToList()
        };
    }
}