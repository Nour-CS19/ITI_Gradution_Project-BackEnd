using Femora.Application.Common.Interfaces;
using Femora.Domain.Entities.LMS;
using Femora.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.LMS.Modules.Commands.CreateModule;

public class CreateModuleHandler : IRequestHandler<CreateModuleCommand, Guid>
{
    private readonly IAppDbContext _context;

    public CreateModuleHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(CreateModuleCommand request, CancellationToken cancellationToken)
    {
        var course = await _context.Courses
            .FirstOrDefaultAsync(x => x.Id == request.CourseId, cancellationToken);

        if (course == null)
            throw new Exception("Course not found");

        if (course.Status == CourseStatus.UnderReview)
            throw new InvalidOperationException("Cannot add modules while the course is under review.");

        var module = new Module
        {
            CourseId = request.CourseId,
            Title = request.Title,
            OrderIndex = request.OrderIndex
        };

        _context.Modules.Add(module);
        await _context.SaveChangesAsync(cancellationToken);

        return module.Id;
    }
}