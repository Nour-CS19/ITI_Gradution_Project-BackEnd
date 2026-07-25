using Femora.Application.Common.Models;
using MediatR;

namespace Femora.Application.Features.Admin.Queries.GetAllProducts;

public sealed record GetAllProductsQuery : IRequest<PagedResult<AdminProductDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
