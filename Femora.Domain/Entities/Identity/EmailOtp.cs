using Femora.Domain.Common;

namespace Femora.Domain.Entities.Identity;

public class EmailOtp : BaseEntity
{
    public Guid   UserId    { get; set; }
    public string Code      { get; set; } = string.Empty;  
    public DateTime ExpiresAt { get; set; }
    public bool   IsUsed    { get; set; } = false;
    public bool   IsExpired => DateTime.UtcNow >= ExpiresAt;

    public ApplicationUser User { get; set; } = null!;
}
