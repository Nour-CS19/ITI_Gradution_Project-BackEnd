using Femora.Application.Features.MarketPlace.Categories.DTOs;
using MediatR;
using System.Collections.Generic;

namespace Femora.Application.Features.MarketPlace.Categories.Queries.GetProductCategories
{
    /// <summary>
    /// Returns every product category (id + name), each annotated with the number of
    /// currently-published products in it — powers the category filter dropdown on the
    /// product catalog page.
    /// </summary>
    public record GetProductCategoriesQuery : IRequest<List<ProductCategoryDto>>;
}
