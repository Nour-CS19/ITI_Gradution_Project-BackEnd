using System;
using System.Collections.Generic;

namespace Femora.Application.Features.MarketPlace.Dtos
{
    /// <summary>
    /// Flat, frontend-friendly shape for the cart. The raw <see cref="Femora.Domain.Entities.Marketplace.Cart"/>
    /// entity does NOT expose pricing/name/image fields at the item level (those live on nested
    /// ProductVariant/Product navigation properties), so returning the entity directly from the
    /// API left the cart/checkout UI with no price to show — every line rendered as free.
    /// This DTO carries the resolved unit price, line total, product name and image explicitly.
    /// </summary>
    public record CartDto
    {
        public Guid Id { get; init; }

        public List<CartItemDto> Items { get; init; } = new();

        public decimal Total { get; init; }
    }

    public record CartItemDto
    {
        public Guid CartItemId { get; init; }

        public Guid ProductId { get; init; }

        public Guid ProductVariantId { get; init; }

        public string ProductName { get; init; } = string.Empty;

        public string? VariantLabel { get; init; }

        public string? ImageUrl { get; init; }

        public int Quantity { get; init; }

        public decimal UnitPrice { get; init; }

        public decimal LineTotal { get; init; }
    }
}
