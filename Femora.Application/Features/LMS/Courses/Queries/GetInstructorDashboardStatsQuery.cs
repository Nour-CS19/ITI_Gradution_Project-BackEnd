using Femora.Application.Features.LMS.Courses.DTOs;
using MediatR;

namespace Femora.Application.Features.LMS.Courses.Queries
{
    /// <summary>Stats + recent courses for the current instructor's dashboard.</summary>
    public record GetInstructorDashboardStatsQuery : IRequest<InstructorDashboardStatsDto>;
}
