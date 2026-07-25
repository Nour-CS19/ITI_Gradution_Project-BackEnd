using MediatR;

namespace Femora.Application.Features.Identity.Commands.Logout;
public sealed record LogoutCommand(Guid UserId, string RefreshToken) : IRequest;

