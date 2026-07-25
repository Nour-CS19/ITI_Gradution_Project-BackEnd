using Femora.Application.Common.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace Femora.Application.Common.Interfaces.Repositories;

public interface IPriceSuggestionRepository
{
    /// <summary>
    /// Suggests a fair market price (in EGP) for a product based on its
    /// name, description, and category, using the assistant's knowledge
    /// of the Egyptian e-commerce market.
    /// </summary>
    Task<AISuggestedPrice> SuggestPriceAsync(
        string productName,
        string? description,
        string categoryName,
        CancellationToken cancellationToken = default);
}
