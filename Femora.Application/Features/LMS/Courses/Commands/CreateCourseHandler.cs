using MediatR;
using Femora.Application.Common.Interfaces;
using Femora.Application.Features.Approvals.Common;
using Femora.Domain.Entities.Admin;
using Femora.Domain.Entities.LMS;
using Femora.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.LMS.Courses.Commands;

public class CreateCourseHandler : IRequestHandler<CreateCourseCommand, Guid>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateCourseHandler(IAppDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateCourseCommand request, CancellationToken cancellationToken)
    {
        var instructorProfileId = request.InstructorProfileId;
        if (instructorProfileId == Guid.Empty)
        {
            var userId = _currentUser.UserId;
            var dbProfile = await _context.InstructorProfiles
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
            if (dbProfile != null)
            {
                instructorProfileId = dbProfile.Id;
            }
        }

        var course = new Course
        {
            Id = Guid.NewGuid(),
            InstructorProfileId = instructorProfileId,
            Title = request.Title,
            Description = request.Description,
            Price = request.Price,
            Category = request.Category,
            Level = request.Level,
            Language = request.Language,
            ThumbnailUrl = request.ThumbnailUrl,
            IsPublished = false,
            RequiresApproval = false,
            Status = CourseStatus.Draft
        };

        await _context.Courses.AddAsync(course, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return course.Id;
    }
}