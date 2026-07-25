using System;
using System.Collections.Generic;

namespace Femora.Application.Features.MarketPlace.Products.DTOs
{
    public record MyProductSummaryDto
    {
        public Guid Id { get; init; }

        public string Name { get; init; } = string.Empty;

        public string? Description { get; init; }

        public string? ImageUrl { get; init; }

        public List<string> ImageUrls { get; init; } = [];

        public decimal MinPrice { get; init; }

        public int TotalStock { get; init; }

        public Guid CategoryId { get; init; }

        public string? CategoryName { get; init; }

        public bool IsPublished { get; init; }

        /// <summary>Draft | PendingApproval | Approved | Rejected</summary>
        public string Status { get; init; } = "Draft";

        public string? AdminNote { get; init; }
    }
}
