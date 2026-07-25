using Femora.Domain.Common;
using Femora.Domain.Entities.LMS;
using Femora.Domain.Entities.Marketplace;
using Femora.Domain.Enums;

namespace Femora.Domain.Entities.Identity;
public class SellerProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public string StoreName { get; set; } = string.Empty;
    public string StoreDescription { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? CoverImageUrl { get; set; }
    public string? BusinessAddress { get; set; }
    public string? BusinessPhone { get; set; }
    public string? ContactEmail { get; set; }
    public string? TaxId { get; set; }
    public float Rating { get; set; }
    public decimal TotalEarnings { get; set; }
    public decimal TaxAmount { get; set; }
    public VerificationStatus Status { get; set; } = VerificationStatus.Pending;
    public Guid? VerifiedByAdminId { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public ApplicationUser User { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<SellerEarning> Earnings { get; set; } = new List<SellerEarning>();
}
