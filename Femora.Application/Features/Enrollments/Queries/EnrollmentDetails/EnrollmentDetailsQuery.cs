using Femora.Application.Features.Enrollments.Common.DTOs;
using MediatR;

namespace Femora.Application.Features.Enrollments.Queries.EnrollmentDetails;

public sealed record EnrollmentDetailsQuery(Guid EnrollmentId) : IRequest<EnrollmentDetailsResponse>;