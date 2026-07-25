using Femora.Application.Features.SellerProfiles.Common;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Femora.Application.Features.SellerProfiles.Commands.UpdateSellerProfile
{
    public record UpdateSellerProfileCommand : IRequest<SellerProfileDto>
    {
        public Guid UserId { get; init; }
        public string StoreName { get; init; } = string.Empty;
        public string? StoreDescription { get; init; }
        public string? BusinessAddress { get; init; }
        public string? BusinessPhone { get; init; }
        public string? ContactEmail { get; init; }
        public string? TaxId { get; init; }
        public IFormFile? Logo { get; init; }
        public IFormFile? CoverImage { get; init; }
    }
}
