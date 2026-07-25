using Femora.Domain.Entities.Onboarding;
using Microsoft.EntityFrameworkCore;

namespace Femora.Infrastructure.Persistence.Seeders
{
    public static class OnboardingInterestSeeder
    {
        public static void SeedOnboardingInterests(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OnboardingInterest>().HasData(
                new OnboardingInterest
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    NameAr = "الكروشيه والتريكو",
                    NameEn = "Crochet & Knitting",
                    DescriptionAr = "صنع الملابس، الإكسسوارات، والديكور بالخيوط",
                    DescriptionEn = "Crochet clothing, accessories and home décor",
                    DisplayOrder = 1,
                    IsActive = true
                },

                new OnboardingInterest
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    NameAr = "التطريز والخياطة",
                    NameEn = "Embroidery & Sewing",
                    DescriptionAr = "التطريز اليدوي، الخياطة، وتفصيل الملابس",
                    DescriptionEn = "Hand embroidery, sewing and tailoring",
                    DisplayOrder = 2,
                    IsActive = true
                },

                new OnboardingInterest
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    NameAr = "المنتجات المنزلية",
                    NameEn = "Home Products",
                    DescriptionAr = "تنظيم المنزل، الديكور، والمفروشات اليدوية",
                    DescriptionEn = "Home décor, organization and handmade furnishings",
                    DisplayOrder = 3,
                    IsActive = true
                },

                new OnboardingInterest
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    NameAr = "الشموع والصابون",
                    NameEn = "Candles & Soap",
                    DescriptionAr = "صناعة الشموع المعطرة والصابون الطبيعي",
                    DescriptionEn = "Handmade candles and natural soap making",
                    DisplayOrder = 4,
                    IsActive = true
                },

                new OnboardingInterest
                {
                    Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                    NameAr = "الإكسسوارات والمجوهرات",
                    NameEn = "Accessories & Jewelry",
                    DescriptionAr = "تصميم الإكسسوارات والمجوهرات اليدوية",
                    DescriptionEn = "Handmade accessories and jewelry",
                    DisplayOrder = 5,
                    IsActive = true
                },

                new OnboardingInterest
                {
                    Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                    NameAr = "الطهي والمخبوزات",
                    NameEn = "Cooking & Baking",
                    DescriptionAr = "الحلويات، المخبوزات، والمأكولات المنزلية",
                    DescriptionEn = "Homemade food, desserts and baked goods",
                    DisplayOrder = 6,
                    IsActive = true
                },

                new OnboardingInterest
                {
                    Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                    NameAr = "الرسم والأعمال الفنية",
                    NameEn = "Arts & Painting",
                    DescriptionAr = "الرسم، التلوين، والأعمال الفنية اليدوية",
                    DescriptionEn = "Painting, drawing and handmade artwork",
                    DisplayOrder = 7,
                    IsActive = true
                },

                new OnboardingInterest
                {
                    Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                    NameAr = "إعادة التدوير والحرف",
                    NameEn = "Recycling & Crafts",
                    DescriptionAr = "إعادة تدوير الخامات وصناعة منتجات يدوية",
                    DescriptionEn = "Recycling materials into handmade products",
                    DisplayOrder = 8,
                    IsActive = true
                }
            );
        }
    }
}