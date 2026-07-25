using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Application.Common.Interfaces.Repositories;
using Femora.Application.Features.SellerProfiles.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.SellerProfiles.Commands.UpdateSellerProfile
{
    public class UpdateSellerProfileCommandHandler(IAppDbContext db, IBlobStorageRepository blobStorage)
        : IRequestHandler<UpdateSellerProfileCommand, SellerProfileDto>
    {
        public async Task<SellerProfileDto> Handle(
            UpdateSellerProfileCommand request,
            CancellationToken cancellationToken)
        {
            var profile = await db.SellerProfiles
                .Include(sp => sp.Products)
                .FirstOrDefaultAsync(sp => sp.UserId == request.UserId, cancellationToken)
                ?? throw new NotFoundException("SellerProfile", request.UserId.ToString());

            profile.StoreName = request.StoreName;
            profile.StoreDescription = request.StoreDescription ?? string.Empty;
            profile.BusinessAddress = request.BusinessAddress;
            profile.BusinessPhone = request.BusinessPhone;
            profile.ContactEmail = request.ContactEmail;
            profile.TaxId = request.TaxId;

            if (request.Logo is not null)
            {
                await using var logoStream = request.Logo.OpenReadStream();
                var logoUrl = await blobStorage.UploadFileAsync(
                    logoStream,
                    request.Logo.FileName,
                    request.Logo.ContentType,
                    folder: "seller-logos",
                    cancellationToken: cancellationToken);

                profile.LogoUrl = logoUrl;
            }

            if (request.CoverImage is not null)
            {
                await using var coverStream = request.CoverImage.OpenReadStream();
                var coverUrl = await blobStorage.UploadFileAsync(
                    coverStream,
                    request.CoverImage.FileName,
                    request.CoverImage.ContentType,
                    folder: "seller-covers",
                    cancellationToken: cancellationToken);

                profile.CoverImageUrl = coverUrl;
            }

            await db.SaveChangesAsync(cancellationToken);

            return new SellerProfileDto
            {
                Id = profile.Id,
                UserId = profile.UserId,
                StoreName = profile.StoreName,
                StoreDescription = profile.StoreDescription,
                LogoUrl = profile.LogoUrl,
                CoverImageUrl = profile.CoverImageUrl,
                BusinessAddress = profile.BusinessAddress,
                BusinessPhone = profile.BusinessPhone,
                ContactEmail = profile.ContactEmail,
                TaxId = profile.TaxId,
                Rating = profile.Rating,
                TotalEarnings = profile.TotalEarnings,
                Status = profile.Status.ToString(),
                ProductsCount = profile.Products.Count,
                VerifiedAt = profile.VerifiedAt
            };
        }
    }
}
