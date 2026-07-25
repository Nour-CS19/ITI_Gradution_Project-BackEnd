using System;
using Femora.Domain.Common;
using Femora.Domain.Entities.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Femora.Domain.Entities.LMS
{
    public class TraineePreferredCategory : BaseEntity
    {
        public Guid TraineeProfileId { get; set; }

        public Guid CourseCategoryId { get; set; }

        public CourseCategory CourseCategory { get; set; } = null!;

        public TraineeProfile TraineeProfile { get; set; } = null!;
    }
}