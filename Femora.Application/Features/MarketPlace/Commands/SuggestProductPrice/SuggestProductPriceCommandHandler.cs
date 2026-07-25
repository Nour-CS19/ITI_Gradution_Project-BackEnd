using Femora.Application.Common.DTOs;
using Femora.Application.Common.Interfaces.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Femora.Application.Features.Marketplace.Commands.SuggestProductPrice;

public class SuggestProductPriceCommandHandler(IPriceSuggestionRepository priceSuggestionRepository)
    : IRequestHandler<SuggestProductPriceCommand, AISuggestedPrice>
{
    public Task<AISuggestedPrice> Handle(SuggestProductPriceCommand request, CancellationToken cancellationToken)
    {
        return priceSuggestionRepository.SuggestPriceAsync(
            request.ProductName,
            request.Description,
            request.CategoryName,
            cancellationToken);
    }
}
