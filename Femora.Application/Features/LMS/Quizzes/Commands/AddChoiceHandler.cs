using Femora.Application.Common.Interfaces;
using Femora.Domain.Entities.LMS.Quizzes;
using MediatR;

namespace Femora.Application.Features.LMS.Quizzes.Commands;

public class AddChoiceHandler : IRequestHandler<AddChoiceCommand, Guid>
{
    private readonly IAppDbContext _context;

    public AddChoiceHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(AddChoiceCommand request, CancellationToken cancellationToken)
    {
        var choice = new Choice
        {
            Id = Guid.NewGuid(),
            QuestionId = request.QuestionId,
            Text = request.Text,
            Order = request.Order,
            IsCorrect = request.IsCorrect
        };

        _context.Choices.Add(choice);
        await _context.SaveChangesAsync(cancellationToken);

        return choice.Id;
    }
}
