using System.ComponentModel.DataAnnotations;
using Femora.Domain.Common;

namespace Femora.Domain.Entities.Onboarding
{
    public class OnboardingInterest : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string NameAr { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string NameEn { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? DescriptionAr { get; set; }

        [MaxLength(500)]
        public string? DescriptionEn { get; set; }

        [Range(0, int.MaxValue)]
        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
