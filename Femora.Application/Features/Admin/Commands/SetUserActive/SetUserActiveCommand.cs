using MediatR;
using System;

namespace Femora.Application.Features.Admin.Commands.SetUserActive
{
    public sealed record SetUserActiveCommand : IRequest
    {
        public Guid UserId { get; init; }
        public bool IsActive { get; init; }
    }
}
