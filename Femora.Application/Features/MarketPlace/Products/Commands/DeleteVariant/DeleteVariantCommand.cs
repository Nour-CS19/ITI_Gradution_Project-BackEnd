using MediatR;

namespace Femora.Application.Features.MarketPlace.Products.Commands.DeleteVariant
{
    /// <summary>
    /// Removes a variant from a Draft/Rejected product.
    /// Blocked when a pending review exists, or when removing would leave the product with zero variants.
    /// </summary>
    public record DeleteVariantCommand(Guid VariantId) : IRequest;
}
