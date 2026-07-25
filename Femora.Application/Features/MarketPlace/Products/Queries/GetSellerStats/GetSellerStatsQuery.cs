using Femora.Application.Features.MarketPlace.Products.DTOs;
using MediatR;

namespace Femora.Application.Features.MarketPlace.Products.Queries.GetSellerStats
{
    public record GetSellerStatsQuery : IRequest<SellerStatsDto>;
}
