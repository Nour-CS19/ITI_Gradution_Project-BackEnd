using Femora.Application.Features.MarketPlace.Dtos;
using MediatR;
using System;

namespace Femora.Application.Features.MarketPlace.Queries.GetCart
{
    public record GetCartQuery : IRequest<CartDto>
    {
        public Guid UserId { get; init; }
    }
}
