using Femora.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.LMS.Quizzes.Commands;

public class ReorderQuestionsHandler : IRequestHandler<ReorderQuestionsCommand>
{
    private readonly IAppDbContext _context;

    public ReorderQuestionsHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ReorderQuestionsCommand request, CancellationToken cancellationToken)
    {
        var questions = await _context.Questions
            .Where(q => q.QuizId == request.QuizId)
            .ToListAsync(cancellationToken);

        foreach (var item in request.Questions)
        {
            var question = questions.FirstOrDefault(q => q.Id == item.QuestionId);
            if (question != null)
            {
                question.OrderIndex = item.Order;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
