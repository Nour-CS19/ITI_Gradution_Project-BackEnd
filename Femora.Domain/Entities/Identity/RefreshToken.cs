using Femora.Domain.Common;
using Femora.Domain.Entities;
using Femora.Domain.Enums;

namespace Femora.Domain.Entities.Identity;
public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; } = false;
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;
    public ProfileType? ActiveProfile { get; set; }
    public ApplicationUser User { get; set; }
}
