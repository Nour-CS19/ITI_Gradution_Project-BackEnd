using Femora.Domain.Common;
using Femora.Domain.Entities.Identity;
using Femora.Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Femora.Domain.Entities.LMS
{
    public class Certificate : BaseEntity
    {
        [Required]
        public Guid EnrollmentId { get; set; }

        [Required]
        public Guid TraineeProfileId { get; set; }

        [Required]
        public Guid CourseId { get; set; }

        [MaxLength(500)]
        public string? CertificateUrl { get; set; }

        [Required]
        [MaxLength(50)]
        public string VerificationCode { get; set; } = string.Empty;

        [Required]
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey(nameof(EnrollmentId))]
        public Enrollment Enrollment { get; set; } = null!;
        public TraineeProfile TraineeProfile { get; set; } = null!;


    }
}