using Femora.Domain.Common;
using Femora.Domain.Entities.LMS;
using Femora.Domain.Enums;

namespace Femora.Domain.Entities.Identity;
public class InstructorProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public string Specialization { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public float Rating { get; set; }
    public decimal TotalEarnings { get; set; }
    public VerificationStatus Status { get; set; } = VerificationStatus.Pending;
    public Guid? VerifiedByAdminId { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public ApplicationUser User { get; set; }
    public ICollection<Course> Courses { get; set; } = new List<Course>();
    public ICollection<InstructorCredential> Credentials { get; set; } = new List<InstructorCredential>();
    public ICollection<InstructorEarning> Earnings { get; set; } = new List<InstructorEarning>();
}
