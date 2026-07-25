using Femora.Application.Features.Enrollments.Common.DTOs;
using MediatR;

namespace Femora.Application.Features.Enrollments.Queries.IsEnrolled;

public sealed record IsEnrolledQuery(Guid CourseId) : IRequest<IsEnrolledResponse>;