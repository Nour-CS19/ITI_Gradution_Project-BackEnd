using MediatR;

namespace Femora.Application.Features.MarketPlace.Orders.Commands.UpdateOrderStatus
{
    /// <summary>
    /// Seller can advance an order through the allowed transitions:
    /// Pending → Processing → Shipped → Delivered.
    /// They cannot cancel orders or revert status.
    /// </summary>
    public record UpdateOrderStatusCommand(
        Guid OrderId,
        string NewStatus
    ) : IRequest;
}
