using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.AccessControl;
using Femora.Domain.Common;
using Femora.Domain.Enums;


namespace Femora.Domain.Entities.LMS
{
    public class Resource : BaseEntity
    {
        [Required]
        public Guid LessonId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string FileUrl { get; set; } = string.Empty;

        [Required]
        public ResourceType Type { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(LessonId))]
        public Lesson Lesson { get; set; } = null!;
    }
}