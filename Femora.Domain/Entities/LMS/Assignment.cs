using Femora.Domain.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Femora.Domain.Entities.LMS
{
    public class Assignment : BaseEntity
    {
        [Required]
        public Guid CourseId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Instructions { get; set; } = string.Empty;

        public DateTime? DueDate { get; set; }

        [Required]
        [Range(1, 100)]
        public int MaxScore { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(CourseId))]
        public Course Course { get; set; } = null!;

        public ICollection<AssignmentSubmission> Submissions { get; set; } = [];
    }
}