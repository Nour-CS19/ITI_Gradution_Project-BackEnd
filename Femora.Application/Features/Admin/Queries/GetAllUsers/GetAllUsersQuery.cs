using Femora.Application.Common.Models;
using MediatR;

namespace Femora.Application.Features.Admin.Queries.GetAllUsers
{
    public sealed record GetAllUsersQuery : IRequest<PagedResult<AdminUserDto>>
    {
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 20;
        /// <summary>Matches against first name, last name, or email.</summary>
        public string? Search { get; init; }
        /// <summary>Optional role filter, e.g. "Admin", "Seller", "Instructor", "Trainee".</summary>
        public string? Role { get; init; }
    }
}
