using Femora.Application.Common.Interfaces;
using Femora.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.Admin.Queries.GetAllOrders
{
    public sealed class GetAllOrdersQueryHandler(IAppDbContext db)
        : IRequestHandler<GetAllOrdersQuery, PagedResult<AdminOrderDto>>
    {
        public async Task<PagedResult<AdminOrderDto>> Handle(
            GetAllOrdersQuery request,
            CancellationToken cancellationToken)
        {
            var query = db.Orders.AsNoTracking().Include(o => o.User).AsQueryable();

            if (request.Status.HasValue)
            {
                query = query.Where(o => o.Status == request.Status.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var pageSize = Math.Clamp(request.PageSize, 1, 100);
            var pageNumber = Math.Max(1, request.PageNumber);

            var items = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new AdminOrderDto
                {
                    Id = o.Id,
                    UserId = o.UserId,
                    BuyerName = o.User != null ? $"{o.User.FirstName} {o.User.LastName}" : "مستخدم محذوف",
                    BuyerEmail = o.User != null ? (o.User.Email ?? string.Empty) : string.Empty,
                    Status = o.Status,
                    TotalAmount = o.TotalAmount,
                    ItemCount = o.OrderItems.Count,
                    CreatedAt = o.CreatedAt,
                })
                .ToListAsync(cancellationToken);

            return new PagedResult<AdminOrderDto>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
            };
        }
    }
}
