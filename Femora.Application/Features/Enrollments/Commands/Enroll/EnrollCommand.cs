using Femora.Application.Features.Enrollments.Common.DTOs;
using MediatR;

namespace Femora.Application.Features.Enrollments.Commands.Enroll;
public sealed record EnrollCommand(Guid CourseId) : IRequest<EnrollmentResponse>;
