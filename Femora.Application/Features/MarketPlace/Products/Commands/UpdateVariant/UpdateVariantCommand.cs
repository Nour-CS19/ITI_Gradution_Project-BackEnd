using MediatR;

namespace Femora.Application.Features.MarketPlace.Products.Commands.UpdateVariant
{
    /// <summary>
    /// Updates a single variant's fields. VariantId comes from the route.
    /// The product must be owned by the current seller and not under pending review.
    /// </summary>
    public record UpdateVariantCommand(
        Guid VariantId,
        string Name,
        decimal Price,
        int StockQuantity,
        string? Color,
        string? Size,
        string? Material
    ) : IRequest;
}
