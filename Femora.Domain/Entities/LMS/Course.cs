using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Femora.Domain.Common;
using Femora.Domain.Entities.Identity;
using Femora.Domain.Enums;

namespace Femora.Domain.Entities.LMS
{
    public class Course : BaseEntity
    {
        [Required]
        public Guid InstructorProfileId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(3000)]
        public string? Description { get; set; }

        [MaxLength(500)]
        public string? ThumbnailUrl { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        [Required]
        [Range(0, 99999)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required]
        [MaxLength(100)]
        public string Category { get; set; } = string.Empty;

        [Required]
        public CourseLevel? Level { get; set; }

        [Required]
        [MaxLength(50)]
        public string Language { get; set; } = string.Empty;

        public bool IsPublished { get; set; } = false;

        public bool RequiresApproval { get; set; } = false;

        public bool IsArchived { get; set; } = false;

        public CourseStatus Status { get; set; } = CourseStatus.Draft;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation Properties
        public ICollection<Module> Modules { get; set; } = new List<Module>();
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
        public ICollection<CourseReview> Reviews { get; set; } = new List<CourseReview>();
        public ICollection<InstructorEarning> InstructorEarnings { get; set; } = new List<InstructorEarning>();
        public InstructorProfile InstructorProfile { get; set; } = null!;
    }

}