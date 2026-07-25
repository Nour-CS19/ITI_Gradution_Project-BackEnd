using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Femora.Domain.Common;
using Femora.Domain.Entities.Identity;
using Femora.Domain.Enums;

namespace Femora.Domain.Entities.LMS
{
    public class InstructorEarning : BaseEntity
    {
        [Required]
        public Guid InstructorProfileId { get; set; }

        [Required]
        public Guid EnrollmentId { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PlatformFee { get; set; }

        [Required]
        public EarningStatus Status { get; set; } = EarningStatus.Pending;

        [Required]
        public DateTime EarnedAt { get; set; } = DateTime.UtcNow;

        public DateTime? PaidAt { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(EnrollmentId))]
        public Enrollment Enrollment { get; set; } = null!;
        public InstructorProfile InstructorProfile { get; set; }

        public static decimal CalculatePlatformFee(decimal price) => price * 0.30m;
    }
}