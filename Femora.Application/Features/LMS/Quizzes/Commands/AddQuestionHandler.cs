using Femora.Application.Common.Interfaces;
using Femora.Domain.Entities.LMS.Quizzes;
using MediatR;

namespace Femora.Application.Features.LMS.Quizzes.Commands;

public class AddQuestionHandler : IRequestHandler<AddQuestionCommand, Guid>
{
    private readonly IAppDbContext _context;

    public AddQuestionHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(AddQuestionCommand request, CancellationToken cancellationToken)
    {
        var question = new Question
        {
            Id = Guid.NewGuid(),
            QuizId = request.QuizId,
            Text = request.Text,
            OrderIndex = request.OrderIndex
        };

        _context.Questions.Add(question);
        await _context.SaveChangesAsync(cancellationToken);

        return question.Id;
    }
}
