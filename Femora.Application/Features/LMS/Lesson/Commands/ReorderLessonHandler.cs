using Femora.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.LMS.Lesson.Commands;

public class ReorderLessonHandler : IRequestHandler<ReorderLessonCommand>
{
    private readonly IAppDbContext _context;

    public ReorderLessonHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ReorderLessonCommand request, CancellationToken cancellationToken)
    {
        var lesson = await _context.Lessons
            .FirstOrDefaultAsync(x => x.Id == request.LessonId, cancellationToken);

        if (lesson == null)
            throw new Exception("Lesson not found");

        lesson.OrderIndex = request.NewOrderIndex;

        await _context.SaveChangesAsync(cancellationToken);
    }
}