using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Application.Features.MarketPlace.Products.DTOs
{
    public record ProductSummaryDto
    {
        public Guid Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public string? Description { get; init; }

        public string? ImageUrl { get; init; }

        public List<string> ImageUrls { get; init; } = [];

        public decimal MinPrice { get; init; }

        public Guid CategoryId { get; init; }

        public string? CategoryName { get; init; }
    }
}

