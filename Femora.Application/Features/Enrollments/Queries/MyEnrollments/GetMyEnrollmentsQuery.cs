using Femora.Application.Features.Enrollments.Common.DTOs;
using MediatR;

namespace Femora.Application.Features.Enrollments.Queries.GetMyEnrollments;

public sealed record GetMyEnrollmentsQuery(int Page = 1, int PageSize = 10) : IRequest<PagedResponse<EnrollmentDTO>>;