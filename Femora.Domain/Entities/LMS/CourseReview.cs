using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Femora.Domain.Common;
namespace Femora.Domain.Entities.LMS
{
    public class CourseReview : BaseEntity
    {
        [Required]
        public Guid CourseId { get; set; }

        [Required]
        public Guid TraineeProfileId { get; set; }

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        [ForeignKey(nameof(CourseId))]
        public Course Course { get; set; } = null!;
    }
}