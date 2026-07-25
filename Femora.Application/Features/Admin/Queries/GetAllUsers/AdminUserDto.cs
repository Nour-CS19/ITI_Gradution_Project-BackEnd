using System;
using System.Collections.Generic;

namespace Femora.Application.Features.Admin.Queries.GetAllUsers
{
    public record AdminUserDto
    {
        public Guid Id { get; init; }
        public string FullName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public List<string> Roles { get; init; } = new();
        public bool IsActive { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}
