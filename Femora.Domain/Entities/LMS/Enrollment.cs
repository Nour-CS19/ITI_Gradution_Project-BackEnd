using System;
using System.Collections.Generic;
using Femora.Domain.Common;
using Femora.Domain.Entities.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Femora.Domain.Entities.LMS
{
    public class Enrollment : BaseEntity
    {
        [Required]
        public Guid TraineeProfileId { get; set; }

        [Required]
        public Guid CourseId { get; set; }

        [Required]
        [Range(0, 99999)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePaid { get; set; }

        [Required]
        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

        public bool IsCompleted { get; set; } = false;

        public DateTime? CompletedAt { get; set; }

        // Navigation Properties
        [ForeignKey(nameof(CourseId))]
        public Course Course { get; set; } = null!;

        public Certificate? Certificate { get; set; }

        public ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();

        public ICollection<InstructorEarning> InstructorEarnings { get; set; } = new List<InstructorEarning>();
        public TraineeProfile TraineeProfile { get; set; } = null!;

        public ICollection<EnrollmentModule> EnrollmentModules  { get; set; } = new List<EnrollmentModule>();

    }
}