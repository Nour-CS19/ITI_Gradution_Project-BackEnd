using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Application.Features.SellerProfiles.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.SellerProfiles.Queries.GetMySellerProfile
{
    public class GetMySellerProfileQueryHandler(
        IAppDbContext db,
        ICurrentUserService currentUser)
        : IRequestHandler<GetMySellerProfileQuery, SellerProfileDto>
    {
        public async Task<SellerProfileDto> Handle(
            GetMySellerProfileQuery request,
            CancellationToken cancellationToken)
        {
            var profile = await db.SellerProfiles
                .AsNoTracking()
                .Where(sp => sp.UserId == currentUser.UserId)
                .Select(sp => new SellerProfileDto
                {
                    Id = sp.Id,
                    UserId = sp.UserId,
                    StoreName = sp.StoreName,
                    StoreDescription = sp.StoreDescription,
                    LogoUrl = sp.LogoUrl,
                    CoverImageUrl = sp.CoverImageUrl,
                    BusinessAddress = sp.BusinessAddress,
                    BusinessPhone = sp.BusinessPhone,
                    ContactEmail = sp.ContactEmail,
                    TaxId = sp.TaxId,
                    Rating = sp.Rating,
                    TotalEarnings = sp.TotalEarnings,
                    Status = sp.Status.ToString(),
                    ProductsCount = sp.Products.Count,
                    VerifiedAt = sp.VerifiedAt
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (profile is null)
            {
                throw new NotFoundException("SellerProfile", currentUser.UserId.ToString());
            }

            return profile;
        }
    }
}
