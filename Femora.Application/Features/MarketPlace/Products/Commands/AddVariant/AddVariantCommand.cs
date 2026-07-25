using MediatR;

namespace Femora.Application.Features.MarketPlace.Products.Commands.AddVariant
{
    /// <summary>
    /// Adds one variant to an existing Draft/Rejected product.
    /// ProductId is resolved from the route; all variant fields come from the request body.
    /// </summary>
    public record AddVariantCommand(
        Guid ProductId,
        string Name,
        decimal Price,
        int StockQuantity,
        string? Color,
        string? Size,
        string? Material
    ) : IRequest<Guid>;
}
