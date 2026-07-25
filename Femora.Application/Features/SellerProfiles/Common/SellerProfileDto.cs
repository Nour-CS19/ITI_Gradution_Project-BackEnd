using System;

namespace Femora.Application.Features.SellerProfiles.Common
{
    public record SellerProfileDto
    {
        public Guid Id { get; init; }
        public Guid UserId { get; init; }
        public string StoreName { get; init; } = string.Empty;
        public string? StoreDescription { get; init; }
        public string? LogoUrl { get; init; }
        public string? CoverImageUrl { get; init; }
        public string? BusinessAddress { get; init; }
        public string? BusinessPhone { get; init; }
        public string? ContactEmail { get; init; }
        public string? TaxId { get; init; }
        public float Rating { get; init; }
        public decimal TotalEarnings { get; init; }
        public string Status { get; init; } = string.Empty;
        public int ProductsCount { get; init; }
        public DateTime? VerifiedAt { get; init; }
    }
}
