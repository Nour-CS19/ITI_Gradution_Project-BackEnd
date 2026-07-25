using Femora.Domain.Enums;
using MediatR;
using System;

namespace Femora.Application.Features.Admin.Commands.UpdateOrderStatus
{
    public sealed record UpdateOrderStatusCommand : IRequest
    {
        public Guid OrderId { get; init; }
        public OrderStatus Status { get; init; }
    }
}
