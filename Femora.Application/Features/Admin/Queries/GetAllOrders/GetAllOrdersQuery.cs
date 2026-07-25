using Femora.Application.Common.Models;
using Femora.Domain.Enums;
using MediatR;

namespace Femora.Application.Features.Admin.Queries.GetAllOrders
{
    public sealed record GetAllOrdersQuery : IRequest<PagedResult<AdminOrderDto>>
    {
        public int PageNumber { get; init; } = 1;
        public int PageSize { get; init; } = 20;
        public OrderStatus? Status { get; init; }
    }
}
