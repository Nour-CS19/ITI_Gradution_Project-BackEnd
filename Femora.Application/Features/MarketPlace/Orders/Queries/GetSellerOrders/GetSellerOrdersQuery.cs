using Femora.Application.Common.Models;
using Femora.Application.Features.MarketPlace.Orders.DTOs;
using MediatR;

namespace Femora.Application.Features.MarketPlace.Orders.Queries.GetSellerOrders
{
    /// <summary>
    /// Returns all orders that contain at least one item from the current seller's products.
    /// Filtered by Status or Search (customer name / order number) if provided.
    /// </summary>
    public record GetSellerOrdersQuery(
        string? Status = null,
        string? Search = null,
        int PageNumber = 1,
        int PageSize = 20
    ) : IRequest<PagedResult<SellerOrderDto>>;
}
