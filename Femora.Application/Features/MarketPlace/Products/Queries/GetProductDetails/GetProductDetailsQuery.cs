using Femora.Application.Features.MarketPlace.Products.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Application.Features.MarketPlace.Products.Queries.GetProductDetails
{


    public record GetProductDetailsQuery(Guid ProductId)
        : IRequest<ProductDetailsDto>;
}
