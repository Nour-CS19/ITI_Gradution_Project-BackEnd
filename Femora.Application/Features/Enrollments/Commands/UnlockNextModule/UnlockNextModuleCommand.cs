using Femora.Application.Features.Enrollments.Common.DTOs;
using MediatR;

namespace Femora.Application.Features.Enrollments.Commands.UnlockNextModule;

public sealed record UnlockNextModuleCommand (Guid ModuleId): IRequest<UnlockNextModuleResponse>;