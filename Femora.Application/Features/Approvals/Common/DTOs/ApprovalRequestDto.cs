using Femora.Domain.Enums;

namespace Femora.Application.Features.Approvals.Common.DTOs;

public class ApprovalRequestDto
{
    public Guid Id { get; set; }
    public ApprovalEntityType Type { get; set; }
    public ApprovalStatus Status { get; set; } = ApprovalStatus.Pending;
    public Guid UserId { get; set; }
    public string RequesterName { get; set; } = string.Empty;
    public string? UserFullName { get; set; }
    public string? UserEmail { get; set; }
    public string? Bio { get; set; }
    public string? PortfolioUrl { get; set; }
    public string? ShopName { get; set; }
    public string? Description { get; set; }
    public Guid? EntityId { get; set; }
    public string? Title { get; set; }
    public DateTime CreatedAt { get; set; }
}