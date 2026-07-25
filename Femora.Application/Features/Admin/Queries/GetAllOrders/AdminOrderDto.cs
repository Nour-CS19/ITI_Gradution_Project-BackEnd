using Femora.Domain.Enums;
using System;

namespace Femora.Application.Features.Admin.Queries.GetAllOrders
{
    public record AdminOrderDto
    {
        public Guid Id { get; init; }
        public Guid UserId { get; init; }
        public string BuyerName { get; init; } = string.Empty;
        public string BuyerEmail { get; init; } = string.Empty;
        public OrderStatus Status { get; init; }
        public decimal TotalAmount { get; init; }
        public int ItemCount { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}
