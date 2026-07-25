using Femora.Domain.Common;

namespace Femora.Domain.Entities.LMS;
public class EnrollmentModule : BaseEntity
{
    public Guid EnrollmentId { get; set; }
    public Guid ModuleId { get; set; }
    public bool IsUnlocked { get; set; }
    public DateTime? UnlockedAt { get; set; }
    public Enrollment Enrollment { get; set; } = null!;
    public Module Module { get; set; } = null!;

    public void Unlock()
    {
        IsUnlocked = true;
        UnlockedAt = DateTime.UtcNow;
    }
}
