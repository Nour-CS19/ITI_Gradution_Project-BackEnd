using System;
using Femora.Domain.Common;
using Femora.Domain.Enums;

namespace Femora.Domain.Entities.Identity;

public class ProfileApplicationRequest : BaseEntity
{
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;
    
    public RequestedRole RequestedRole { get; set; }
    public ApplicationRequestStatus Status { get; set; }
    
    public DateTime SubmittedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public Guid? ReviewedByAdminId { get; set; }
    public ApplicationUser? ReviewedByAdmin { get; set; }
    
    public string? RejectionReason { get; set; }
    
    public string? Bio { get; set; }
    public string? PortfolioUrl { get; set; }
    public string? NationalIdNumber { get; set; }
    
    // Seller only fields
    public string? StoreName { get; set; }
    public string? StoreDescription { get; set; }
}
