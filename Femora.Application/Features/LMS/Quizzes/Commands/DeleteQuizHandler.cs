using Femora.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.LMS.Quizzes.Commands;

public class DeleteQuizHandler : IRequestHandler<DeleteQuizCommand>
{
    private readonly IAppDbContext _context;

    public DeleteQuizHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteQuizCommand request, CancellationToken cancellationToken)
    {
        var quiz = await _context.Quizzes
            .FirstOrDefaultAsync(q => q.Id == request.QuizId, cancellationToken);

        if (quiz == null)
            throw new Exception("Quiz not found");

        _context.Quizzes.Remove(quiz);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
