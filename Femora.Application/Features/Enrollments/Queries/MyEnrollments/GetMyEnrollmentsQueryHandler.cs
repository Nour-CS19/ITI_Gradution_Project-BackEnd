using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Application.Common.Interfaces.Repositories.LMS;
using Femora.Application.Features.Enrollments.Common.DTOs;
using Femora.Domain.Entities.Identity;
using Femora.Domain.Entities.LMS;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.Enrollments.Queries.GetMyEnrollments;

public class GetMyEnrollmentsQueryHandler(IAppDbContext _context,
                                 IEnrollmentRepository _enrollmentRepo,
                                 ICurrentUserService _currentUser)
                            : IRequestHandler<GetMyEnrollmentsQuery, PagedResponse<EnrollmentDTO>>
{
    public async Task<PagedResponse<EnrollmentDTO>> Handle(GetMyEnrollmentsQuery request, CancellationToken cancellationToken)
    {
        var traineeProfile = await _context.TraineeProfiles.FirstOrDefaultAsync(tp => tp.UserId == _currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(TraineeProfile), _currentUser.UserId.ToString());

        // GetMyEnrollmentsProjected translates straight to SQL (including the lesson/
        // progress counts), so only the current page of small DTOs is ever materialized
        // -- the previous version loaded every enrolled course's full Modules/Lessons/
        // LessonProgresses graph before paginating in memory.
        var query = _enrollmentRepo.GetMyEnrollmentsProjected(traineeProfile.Id);

        var totalCount = await query.CountAsync(cancellationToken);

        var dtos = await query
            .OrderByDescending(e => e.EnrolledAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // ProgressPercent couldn't be computed inside the SQL translation, so fill it
        // in now -- cheap, since we're only touching the current page (<= PageSize rows).
        var finalDtos = dtos.Select(dto => new EnrollmentDTO
        {
            EnrollmentId = dto.EnrollmentId,
            CourseId = dto.CourseId,
            CourseTitle = dto.CourseTitle,
            ThumbnailUrl = dto.ThumbnailUrl,
            PricePaid = dto.PricePaid,
            EnrolledAt = dto.EnrolledAt,
            IsCompleted = dto.IsCompleted,
            TotalLessons = dto.TotalLessons,
            CompletedLessons = dto.CompletedLessons,
            ProgressPercent = CalculateProgress(dto.CompletedLessons, dto.TotalLessons)
        }).ToList();

        return new PagedResponse<EnrollmentDTO>
        {
            Data = finalDtos,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
        };
    }
    private static int CalculateProgress(int completedLessons, int totalLessons)
    {
        if (totalLessons ==0)
            return 0;

        return (int) Math.Round((double) completedLessons / totalLessons * 100);
    }
}
