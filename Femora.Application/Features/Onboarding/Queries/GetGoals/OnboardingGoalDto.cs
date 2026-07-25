namespace Femora.Application.Features.Onboarding.Queries.GetGoals
{
    public class OnboardingGoalDto
    {
        public Guid Id { get; set; }
        public string LabelAr { get; set; } = string.Empty;
        public string LabelEn { get; set; } = string.Empty;
        public string? DescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }
        public string? Emoji { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }
}
