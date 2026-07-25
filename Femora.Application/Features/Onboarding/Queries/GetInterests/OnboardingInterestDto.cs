namespace Femora.Application.Features.Onboarding.Queries.GetInterests
{
    public class OnboardingInterestDto
    {
        public Guid Id { get; set; }
        public string NameAr { get; set; } = string.Empty;
        public string NameEn { get; set; } = string.Empty;
        public string? DescriptionAr { get; set; }
        public string? DescriptionEn { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; }
    }
}
