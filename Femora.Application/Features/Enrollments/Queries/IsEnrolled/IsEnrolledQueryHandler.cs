using Femora.Application.Common.Interfaces;
using Femora.Application.Features.Enrollments.Common.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.Enrollments.Queries.IsEnrolled;

public class IsEnrolledQueryHandler(IAppDbContext _context, ICurrentUserService _currentUser)
                                  : IRequestHandler<IsEnrolledQuery, IsEnrolledResponse>
{
    public async Task<IsEnrolledResponse> Handle(IsEnrolledQuery request, CancellationToken cancellationToken)
    {
        var traineeProfile = await _context.TraineeProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(tp => tp.UserId == _currentUser.UserId, cancellationToken);

        if (traineeProfile == null)
            return new IsEnrolledResponse { IsEnrolled = false };

        var enrollment = await _context.Enrollments
                        .AsNoTracking()
                        .FirstOrDefaultAsync(e => e.TraineeProfileId == traineeProfile.Id
                                                        && e.CourseId == request.CourseId, cancellationToken);

        if (enrollment is null)
            return new IsEnrolledResponse { IsEnrolled = false };

        return new IsEnrolledResponse
        {
            IsEnrolled = true,
            EnrollmentId = enrollment?.Id,
            IsCompleted = enrollment.IsCompleted
        };
    }
}
