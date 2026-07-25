using Femora.Application.Common.Models;
using Femora.Application.Features.MarketPlace.Products.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Application.Features.MarketPlace.Products.Queries.BrowseProducts
{
    public record BrowseProductsQuery(
     string? Search,
     Guid? CategoryId,
     int PageNumber = 1,
     int PageSize = 10
 ) : IRequest<PagedResult<ProductSummaryDto>>;
}
