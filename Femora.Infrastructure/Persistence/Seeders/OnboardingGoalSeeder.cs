using Femora.Domain.Entities.Onboarding;
using Microsoft.EntityFrameworkCore;

namespace Femora.Infrastructure.Persistence.Seeders
{
    public static class OnboardingGoalSeeder
    {
        public static void SeedOnboardingGoals(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OnboardingGoal>().HasData(
                new OnboardingGoal
                {
                    Id = Guid.Parse("a1111111-1111-1111-1111-111111111111"),
                    Emoji = "🧶",
                    LabelAr = "تعلم حرفة يدوية جديدة",
                    LabelEn = "Learn a new handcraft",
                    DescriptionAr = "اكتسبى مهارة جديدة فى الكروشيه، التطريز، أو الخزف",
                    DescriptionEn = "Pick up a new skill in crochet, embroidery or pottery",
                    DisplayOrder = 1,
                    IsActive = true
                },
                new OnboardingGoal
                {
                    Id = Guid.Parse("a2222222-2222-2222-2222-222222222222"),
                    Emoji = "🎨",
                    LabelAr = "تطوير مهاراتى الحالية",
                    LabelEn = "Improve my current skills",
                    DescriptionAr = "حسّنى مستواكِ فى الحرف التى تمارسينها بالفعل",
                    DescriptionEn = "Level up the crafts you already practice",
                    DisplayOrder = 2,
                    IsActive = true
                },
                new OnboardingGoal
                {
                    Id = Guid.Parse("a3333333-3333-3333-3333-333333333333"),
                    Emoji = "💰",
                    LabelAr = "تحويل هوايتى إلى مصدر دخل",
                    LabelEn = "Turn my hobby into income",
                    DescriptionAr = "ابدئى ببيع منتجاتك اليدوية وكسب دخل ثابت",
                    DescriptionEn = "Start selling your handmade products for steady income",
                    DisplayOrder = 3,
                    IsActive = true
                },
                new OnboardingGoal
                {
                    Id = Guid.Parse("a4444444-4444-4444-4444-444444444444"),
                    Emoji = "🛍️",
                    LabelAr = "بناء مشروع حرفى خاص",
                    LabelEn = "Build my own craft business",
                    DescriptionAr = "أسّسى علامتكِ التجارية ومتجركِ الخاص",
                    DescriptionEn = "Found your own brand and store",
                    DisplayOrder = 4,
                    IsActive = true
                },
                new OnboardingGoal
                {
                    Id = Guid.Parse("a5555555-5555-5555-5555-555555555555"),
                    Emoji = "👩‍🏫",
                    LabelAr = "مشاركة خبرتى وتدريب الأخريات",
                    LabelEn = "Share my expertise and mentor others",
                    DescriptionAr = "علّمى مهاراتكِ وأثّرى فى مجتمع Femora",
                    DescriptionEn = "Teach your skills and inspire the Femora community",
                    DisplayOrder = 5,
                    IsActive = true
                }
            );
        }
    }
}
