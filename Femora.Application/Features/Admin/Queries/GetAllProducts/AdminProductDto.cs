using System;

namespace Femora.Application.Features.Admin.Queries.GetAllProducts;

public record AdminProductDto
{
    public Guid Id { get; init; }
    public Guid SellerProfileId { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal MinPrice { get; init; }
    public bool IsPublished { get; init; }
    public int VariantCount { get; init; }
    public int ImageCount { get; init; }
    public DateTime CreatedAt { get; init; }
}
