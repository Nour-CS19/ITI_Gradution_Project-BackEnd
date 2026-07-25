using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Femora.Domain.Entities.Marketplace;
using Femora.Infrastructure.Data;

namespace Femora.Infrastructure.Persistence.Seeders
{
    internal static class CategorySeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            // Check if product categories already exist
            if (context.ProductCategories.Any())
                return;

            var productCategories = new[]
            {
                new ProductCategory { Name = "منتجات الكروشيه", Description = "منتجات كروشيه يدوية تشمل البطانيات والطرح والقبعات والقطع الزخرفية" },
                new ProductCategory { Name = "ملابس تريكو", Description = "ملابس وإكسسوارات تريكو أنيقة مصنوعة من خيوط عالية الجودة" },
                new ProductCategory { Name = "حقائب يدوية", Description = "حقائب يدوية فريدة تشمل شنط التوت وحقائب الكتف والكلاتش" },
                new ProductCategory { Name = "ديكور منزلي", Description = "قطع ديكور يدوية للمنزل تشمل لوحات الحائط والماكرامي وقطع الزينة المركزية" },
                new ProductCategory { Name = "شموع", Description = "شموع سوي وشموع معطّرة حرفية مصنوعة من مواد طبيعية" },
                new ProductCategory { Name = "صابون طبيعي", Description = "صابون طبيعي يدوي بمكونات عضوية وخصائص علاجية" },
                new ProductCategory { Name = "مجوهرات", Description = "مجوهرات يدوية تشمل الأساور والعقود والحلق والخواتم" },
                new ProductCategory { Name = "منتجات راتنج", Description = "قطع فنية جميلة من الراتنج تشمل الصواني والكوسترات والمرجعيات ولوحات الحائط" },
                new ProductCategory { Name = "فخار وسيراميك", Description = "قطع فخار وسيراميك يدوية تشمل الأكواب والأطباق والأوعية والمزهريات" },
                new ProductCategory { Name = "أطقم حرفية", Description = "أطقم DIY لتعلّم المهارات اليدوية وصنع تحفك الخاصة" }
            };

            context.ProductCategories.AddRange(productCategories);
            await context.SaveChangesAsync();
        }
    }
}