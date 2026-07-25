using Femora.Application.Common.Models;
using Femora.Application.Features.MarketPlace.Products.DTOs;
using MediatR;
using System;

namespace Femora.Application.Features.MarketPlace.Products.Queries.GetMyProducts
{
    /// <summary>
    /// Status filter accepts: Draft | PendingApproval | Approved | Rejected (case-insensitive).
    /// </summary>
    public record GetMyProductsQuery(
        string? Search = null,
        string? Status = null,
        Guid? CategoryId = null,
        int PageNumber = 1,
        int PageSize = 10
    ) : IRequest<PagedResult<MyProductSummaryDto>>;
}
