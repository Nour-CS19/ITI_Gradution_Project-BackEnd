using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Femora.Domain.Common;
using Femora.Domain.Enums;

namespace Femora.Domain.Entities.LMS
{
    public class LessonProgress : BaseEntity
    {
        [Required]
        public Guid EnrollmentId { get; set; }

        [Required]
        public Guid LessonId { get; set; }

        [Required]
        public bool IsCompleted { get; set; } = false;

        [Range(0, int.MaxValue)]
        public int? WatchedSeconds { get; set; }

        public DateTime? LastAccessedAt { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(EnrollmentId))]
        public Enrollment Enrollment { get; set; } = null!;

        [ForeignKey(nameof(LessonId))]
        public Lesson Lesson { get; set; } = null!;
    }
}