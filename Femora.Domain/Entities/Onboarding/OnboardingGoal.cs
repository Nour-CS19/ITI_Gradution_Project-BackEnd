using System.ComponentModel.DataAnnotations;
using Femora.Domain.Common;

namespace Femora.Domain.Entities.Onboarding
{
    public class OnboardingGoal : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string LabelAr { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LabelEn { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? DescriptionAr { get; set; }

        [MaxLength(500)]
        public string? DescriptionEn { get; set; }

        [MaxLength(10)]
        public string? Emoji { get; set; }

        [Range(0, int.MaxValue)]
        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
