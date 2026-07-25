using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Femora.Domain.Entities.LMS;
using Femora.Infrastructure.Data;

namespace Femora.Infrastructure.Persistence.Seeders
{
    internal static class CourseCategorySeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            // Check if categories already exist
            if (context.CourseCategories.Any())
                return;

            var courseCategories = new[]
            {
                new CourseCategory { Name = "الكروشيه والتريكو", Description = "تعلّم فنون الكروشيه والتريكو بدءًا من الغرز الأساسية وصولًا إلى الأنماط والتصاميم المتقدمة." },
                new CourseCategory { Name = "التطريز والخياطة", Description = "أتقن تقنيات التطريز والخياطة اليدوية والآلية لصنع منسوجات وملابس جميلة." },
                new CourseCategory { Name = "فن الراتنج والإيبوكسي", Description = "اكتشف تقنيات صناعة فن الراتنج والمجوهرات والقطع الديكورية المذهلة." },
                new CourseCategory { Name = "الفخار والسيراميك", Description = "تعلّم تقنيات دولاب الفخار والتشكيل اليدوي والتزجيج لصنع تحف سيراميك رائعة." },
                new CourseCategory { Name = "فن الماكرامي", Description = "استكشف فن الماكرامي العريق لصنع لوحات الحائط وعلاقات النباتات والإكسسوارات الزخرفية." },
                new CourseCategory { Name = "صناعة الشموع والصابون", Description = "أتقن حرفة صناعة شموع الصويا والشموع المعطّرة والصابون الطبيعي اليدوي." },
                new CourseCategory { Name = "المجوهرات اليدوية", Description = "اصنع مجوهرات يدوية جميلة تشمل قطع الخرز والأسلاك المجدولة والمعادن المصبوبة." },
                new CourseCategory { Name = "الديكوباج والرسم", Description = "تعلّم تقنيات الديكوباج والرسم الزخرفي على أسطح متعددة تشمل الخشب والأثاث." }
            };

            // Build image URLs for categories using LoremFlickr so seeded data has real-looking images
            var categoryImageKeywords = new Dictionary<string, string[]>
            {
                { "الكروشيه والتريكو", new[] { "crochet,knitting" } },
                { "التطريز والخياطة", new[] { "embroidery,sewing" } },
                { "فن الراتنج والإيبوكسي", new[] { "resin,art" } },
                { "الفخار والسيراميك", new[] { "pottery,ceramic" } },
                { "فن الماكرامي", new[] { "macrame,craft" } },
                { "صناعة الشموع والصابون", new[] { "candle,soap" } },
                { "المجوهرات اليدوية", new[] { "jewelry,handmade" } },
                { "الديكوباج والرسم", new[] { "decoupage,painting" } }
            };

            int imageSeed = 1;
            foreach (var cat in courseCategories)
            {
                if (categoryImageKeywords.TryGetValue(cat.Name, out var keywords) && keywords.Length > 0)
                {
                    cat.ImageUrl = BuildImageUrl(keywords[0], imageSeed++);
                }
                else
                {
                    cat.ImageUrl = BuildImageUrl("handmade,craft", imageSeed++);
                }
            }

            context.CourseCategories.AddRange(courseCategories);
            await context.SaveChangesAsync();
        }

        private static string BuildImageUrl(string keywords, int seed, int width = 800, int height = 450)
            => $"https://loremflickr.com/{width}/{height}/{keywords}?lock={seed}";
    }
}
