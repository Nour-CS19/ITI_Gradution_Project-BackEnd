namespace Femora.Application.Features.Approvals.Common.Requests;

public record ApplySellerRequest
{
    public string ShopName { get; set; } = string.Empty;
    public string? Description { get; set; }
}
