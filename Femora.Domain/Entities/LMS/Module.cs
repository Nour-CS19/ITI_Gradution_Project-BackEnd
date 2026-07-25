using System.ComponentModel.DataAnnotations;
using Femora.Domain.Common;
using Femora.Domain.Entities.LMS.Quizzes;

namespace Femora.Domain.Entities.LMS
{
    public class Module : BaseEntity
    {
        [Required]
        public Guid CourseId { get; set; }
        public Guid? QuizId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Range(1, 1000)]
        public int OrderIndex { get; set; }

        public Course Course { get; set; } = null!;
        public Quiz? Quiz { get; set; }
        public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
        public ICollection<EnrollmentModule> EnrollmentModules { get; set; } = new List<EnrollmentModule>();
    }
}