using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Femora.Domain.Common;
using Femora.Domain.Entities.Identity;
using Femora.Domain.Enums;

namespace Femora.Domain.Entities.LMS
{
    public class AssignmentSubmission : BaseEntity
    {
        [Required]
        public Guid AssignmentId { get; set; }

        [Required]
        public Guid TraineeProfileId { get; set; }

        [MaxLength(500)]
        public string? SubmissionUrl { get; set; }

        [Range(0, 100)]
        public int? Score { get; set; }

        [MaxLength(1000)]
        public string? Feedback { get; set; }

        [Required]
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public DateTime? GradedAt { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(AssignmentId))]
        public Assignment Assignment { get; set; } = null!;
        public TraineeProfile TraineeProfile { get; set; } = null!;
    }
}