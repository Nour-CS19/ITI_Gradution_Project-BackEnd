using Femora.Application.Common.DTOs;
using MediatR;

namespace Femora.Application.Features.Marketplace.Commands.SuggestProductPrice;

public record SuggestProductPriceCommand : IRequest<AISuggestedPrice>
{
    public string ProductName { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string CategoryName { get; init; } = string.Empty;
}
