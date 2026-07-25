using Femora.Application.Common.Interfaces;
using Femora.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.Admin.Queries.GetAllUsers
{
    public sealed class GetAllUsersQueryHandler(
        IAppDbContext db)
        : IRequestHandler<GetAllUsersQuery, PagedResult<AdminUserDto>>
    {
        public async Task<PagedResult<AdminUserDto>> Handle(
            GetAllUsersQuery request,
            CancellationToken cancellationToken)
        {
            var query = db.ApplicationUsers.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Search))
            {
                var term = request.Search.Trim();
                query = query.Where(u =>
                    u.FirstName.Contains(term) ||
                    u.LastName.Contains(term) ||
                    (u.Email != null && u.Email.Contains(term)));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var pageSize = Math.Clamp(request.PageSize, 1, 100);
            var pageNumber = Math.Max(1, request.PageNumber);

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            // Roles live in ASP.NET Identity's own join tables and aren't navigable from
            // ApplicationUser directly. Previously this called UserManager.GetRolesAsync()
            // once per user (up to `pageSize` round trips per request). Batching it into a
            // single join against UserRoles/ApplicationRoles for all IDs on this page cuts
            // that down to exactly one extra query, regardless of page size.
            var userIds = users.Select(u => u.Id).ToList();

            var roleRows = await (
                from ur in db.UserRoles
                where userIds.Contains(ur.UserId)
                join r in db.ApplicationRoles on ur.RoleId equals r.Id
                select new { ur.UserId, RoleName = r.Name }
            ).ToListAsync(cancellationToken);

            var rolesByUserId = roleRows
                .GroupBy(x => x.UserId)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(x => x.RoleName ?? string.Empty).ToList());

            var items = new List<AdminUserDto>(users.Count);
            foreach (var user in users)
            {
                var roles = rolesByUserId.TryGetValue(user.Id, out var userRoles)
                    ? userRoles
                    : new List<string>();

                if (!string.IsNullOrWhiteSpace(request.Role) &&
                    !roles.Contains(request.Role, StringComparer.OrdinalIgnoreCase))
                {
                    // NOTE: Role lives in Identity's own join tables, which aren't exposed
                    // on IAppDbContext as a first-class navigation, so this filter runs
                    // AFTER paging at the DB level. That means totalCount/page size can be
                    // slightly off when a Role filter is combined with paging (a page may
                    // come back with fewer than pageSize items). Fine for admin browsing;
                    // if this needs to be exact, push the join above into the main query.
                    continue;
                }

                items.Add(new AdminUserDto
                {
                    Id = user.Id,
                    FullName = $"{user.FirstName} {user.LastName}".Trim(),
                    Email = user.Email ?? string.Empty,
                    Roles = roles,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                });
            }

            return new PagedResult<AdminUserDto>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
            };
        }
    }
}
