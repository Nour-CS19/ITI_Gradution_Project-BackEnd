using System;

namespace Femora.Application.Features.MarketPlace.Categories.DTOs
{
    public record ProductCategoryDto
    {
        public Guid Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public string? Description { get; init; }

        /// <summary>Number of published products currently in this category — lets the UI hide empty filters.</summary>
        public int ProductCount { get; init; }
    }
}
