using MediatR;
using System;
using System.Collections.Generic;

namespace Femora.Application.Features.Marketplace.Queries.RecommendProducts;

public record RecommendProductsQuery : IRequest<List<RecommendedProductDto>>
{
    public Guid UserId { get; init; }
    public int Top { get; init; } = 10;
}

public record RecommendedProductDto
{
    public Guid ProductId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public decimal? MinPrice { get; init; }
    public string? PrimaryImageUrl { get; init; }
    public double Score { get; init; }
}
