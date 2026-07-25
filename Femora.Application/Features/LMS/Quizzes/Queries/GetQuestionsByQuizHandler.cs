using Femora.Application.Common.Interfaces;
using Femora.Application.Features.LMS.Quizzes.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.LMS.Quizzes.Queries;

public class GetQuestionsByQuizHandler : IRequestHandler<GetQuestionsByQuizQuery, List<QuestionDto>>
{
    private readonly IAppDbContext _context;

    public GetQuestionsByQuizHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<QuestionDto>> Handle(GetQuestionsByQuizQuery request, CancellationToken cancellationToken)
    {
        return await _context.Questions
            .Where(q => q.QuizId == request.QuizId)
            .OrderBy(q => q.OrderIndex)
            .Select(q => new QuestionDto
            {
                Id = q.Id,
                Text = q.Text,
                OrderIndex = q.OrderIndex,
                Choices = q.Choices
                    .OrderBy(c => c.Order)
                    .Select(c => new ChoiceDto
                    {
                        Id = c.Id,
                        Text = c.Text
                    }).ToList()
            })
            .ToListAsync(cancellationToken);
    }
}
