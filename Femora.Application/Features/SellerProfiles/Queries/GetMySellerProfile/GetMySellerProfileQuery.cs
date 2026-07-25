using Femora.Application.Features.SellerProfiles.Common;
using MediatR;

namespace Femora.Application.Features.SellerProfiles.Queries.GetMySellerProfile
{
    public record GetMySellerProfileQuery : IRequest<SellerProfileDto>;
}
