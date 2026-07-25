using Femora.Application.Features.Identity.Common.Policies;
using Femora.Application.Features.SellerProfiles.Commands.UpdateSellerProfile;
using Femora.Application.Features.SellerProfiles.Queries.GetMySellerProfile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Femora.API.Controllers.Identity
{
    [Route("api/seller/profile")]
    [ApiController]
    [Authorize(Policy = Policies.Seller)]
    public class SellerProfileController(IMediator mediator) : ControllerBase
    {
        private Guid CurrentUserId =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User id claim not found."));

        /// <summary>
        /// Gets the current seller's store profile (store info, business info, contact info, stats).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMyStoreProfile(CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetMySellerProfileQuery(), cancellationToken);
            return Ok(result);
        }

        /// <summary>
        /// Updates the current seller's store profile. Accepts multipart/form-data so a
        /// logo and/or cover image can be uploaded alongside the other fields.
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> UpdateMyStoreProfile(
            [FromForm] UpdateSellerProfileRequest request,
            CancellationToken cancellationToken)
        {
            var command = new UpdateSellerProfileCommand
            {
                UserId = CurrentUserId,
                StoreName = request.StoreName,
                StoreDescription = request.StoreDescription,
                BusinessAddress = request.BusinessAddress,
                BusinessPhone = request.BusinessPhone,
                ContactEmail = request.ContactEmail,
                TaxId = request.TaxId,
                Logo = request.Logo,
                CoverImage = request.CoverImage
            };

            var result = await mediator.Send(command, cancellationToken);
            return Ok(result);
        }
    }

    public class UpdateSellerProfileRequest
    {
        public string StoreName { get; set; } = string.Empty;
        public string? StoreDescription { get; set; }
        public string? BusinessAddress { get; set; }
        public string? BusinessPhone { get; set; }
        public string? ContactEmail { get; set; }
        public string? TaxId { get; set; }
        public IFormFile? Logo { get; set; }
        public IFormFile? CoverImage { get; set; }
    }
}
