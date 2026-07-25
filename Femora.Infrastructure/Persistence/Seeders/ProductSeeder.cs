using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Femora.Domain.Entities;
using Femora.Domain.Entities.Identity;
using Femora.Domain.Entities.Marketplace;
using Femora.Domain.Enums;
using Femora.Infrastructure.Data;

namespace Femora.Infrastructure.Persistence.Seeders
{
    internal static class ProductSeeder
    {
        // ════════════════════════════════════════════════════════════
        //  بيانات المنتجات مرتّبة حسب اسم الفئة (مطابق CategorySeeder)
        //  Format: "اسم المنتج|الوصف|سعر أساسي"
        // ════════════════════════════════════════════════════════════
        private static readonly Dictionary<string, string[]> ProductsData = new()
        {
            {
                "منتجات الكروشيه",
                new[]
                {
                    "دبة كروشيه يدوية|دمية دبة أميغورومي مصنوعة يدوياً من خيوط قطنية ناعمة مع حشوة فوم صحية. مناسبة للأطفال والهدايا.|85",
                    "شنطة كروشيه بألوان متدرجة|حقيبة كتف مصنوعة يدوياً من خيوط كروشيه ملونة بأسلوب بوهيمي أصيل.|120",
                    "جوارب كروشيه للأطفال|جوارب دافئة ومريحة مصنوعة يدوياً من خيوط قطنية ناعمة مناسبة للمواليد.|55",
                    "زهور كروشيه للزينة|مجموعة زهور كروشيه يدوية ملونة مناسبة لتزيين الهدايا والمنازل.|40",
                    "بطانية كروشيه للأطفال|بطانية ناعمة ودافئة مصنوعة يدوياً من خيوط مضادة للحساسية بألوان هادئة.|210"
                }
            },
            {
                "ملابس تريكو",
                new[]
                {
                    "بلوفر تريكو بالصوف|بلوفر دافئ مُحاك يدوياً من صوف ميرينو الطبيعي بألوان كلاسيكية أنيقة.|350",
                    "طرحة تريكو ملوّنة|طرحة طويلة وناعمة بنقوش هندسية مُحاكة يدوياً بألوان متناسقة.|95",
                    "كارديجان تريكو أنيق|كارديجان مفتوح مُحاك يدوياً مناسب لجميع المناسبات الرسمية والعادية.|420",
                    "قبعة تريكو للأطفال|قبعة طفل صغيرة ناعمة ومحبوكة يدوياً بخيوط لطيفة على الجلد.|70",
                    "جوارب تريكو بنقوش|جوارب دافئة محبوكة يدوياً بتصاميم ونقوش متعددة وألوان جذابة.|65"
                }
            },
            {
                "حقائب يدوية",
                new[]
                {
                    "شنطة جلد طبيعي توت باج|حقيبة توت كبيرة من الجلد الطبيعي متينة ومناسبة للاستخدام اليومي.|650",
                    "حقيبة قماش مطرّزة|حقيبة كتف واسعة من القماش المتين مزيّنة بتطريز يدوي ملوّن.|180",
                    "سلّة تسوق منسوجة|سلّة تسوق مصنوعة يدوياً من ألياف طبيعية متينة وصديقة للبيئة.|90",
                    "كلتش مطرّز بالتقليدي|حقيبة يد أنيقة مطرّزة يدوياً بنقوش تراثية مناسبة للمناسبات.|155",
                    "حقيبة سهرة بالخرز|حقيبة سهرة فاخرة مزيّنة بخرز وترتر يدوي مناسبة للمناسبات الرسمية.|280"
                }
            },
            {
                "ديكور منزلي",
                new[]
                {
                    "لوحة ماكرامي للحائط|لوحة ماكرامي يدوية من خيط قطني طبيعي لإضفاء لمسة بوهيمية على المنزل.|175",
                    "تابلوه تطريز تراثي|لوحة تطريز يدوية بنقوش مصرية تراثية داخل إطار خشبي أنيق.|220",
                    "بلاط سيراميك يدوي|بلاطات سيراميك مصنوعة يدوياً بنقوش فسيفساء ملوّنة للحوائط والأرفف.|140",
                    "قطعة دانتيل كروشيه|قطعة دانتيل كروشيه أنتيكا بتصميم ريفي أنيق لتزيين الطاولات.|75",
                    "علّاقة نباتات ماكرامي|علّاقة ماكرامي يدوية لتعليق أصص النباتات الداخلية بطريقة ديكورية.|110"
                }
            },
            {
                "شموع",
                new[]
                {
                    "شمعة فانيلا سوي|شمعة سوي يدوية برائحة الفانيلا الطبيعية الدافئة وقت الاحتراق 45 ساعة.|85",
                    "شمعة لافندر معطّرة|شمعة معطّرة بزيت اللافندر الأصيل لإضفاء جو من الاسترخاء والهدوء.|90",
                    "شمعة حدائق الورد|شمعة فاخرة برائحة الورد الطازج مصنوعة بزيوت عطرية طبيعية.|95",
                    "شمعة الياسمين الليلي|شمعة برائحة الياسمين الشرقية الفاتنة في وعاء زجاجي أنيق.|100",
                    "طقم شموع هدية|طقم ثلاث شموع معطّرة بروائح مختلفة في علبة هدية فاخرة.|240"
                }
            },
            {
                "صابون طبيعي",
                new[]
                {
                    "صابون زيت الزيتون|صابون زيت زيتون بكر مصنوع بالطريقة الباردة غني بالمرطبات الطبيعية.|45",
                    "صابون لافندر وعسل|صابون مريح ومعطّر باللافندر والعسل الخام لبشرة ناعمة ومشرقة.|50",
                    "صابون طين الورد|صابون لطيف بطين الورد الوردي مناسب للبشرة الحساسة والجافة.|55",
                    "صابون فحم نباتي|صابون الفحم النباتي المنشط لتنظيف عميق وإزالة الشوائب من المسام.|50",
                    "صابون حدائق الأعشاب|صابون يدوي بمزيج أعشاب طبيعية ونباتات مفيدة للبشرة.|60"
                }
            },
            {
                "مجوهرات",
                new[]
                {
                    "إسورة أحجار شبه كريمة|إسورة جميلة مصنوعة يدوياً من أحجار شبه كريمة طبيعية ملوّنة.|135",
                    "عقد سلك فضي|عقد أنيق مصنوع يدوياً من سلك الفضة 925 بتصاميم فريدة ومميزة.|195",
                    "حلق لؤلؤ طبيعي|حلق كلاسيكي من اللؤلؤ الطبيعي بخطاطيف فضة 925 راقية.|165",
                    "خاتم أحجار كريمة|خاتم أنيق مُرصَّع بحجر طبيعي فريد مصنوع يدوياً بمهارة عالية.|220",
                    "خلخال يدوي مُرصَّع|خلخال رفيع وجميل مصنوع يدوياً بخرز وقلادات صغيرة رقيقة.|95"
                }
            },
            {
                "منتجات راتنج",
                new[]
                {
                    "طقم كوسترات راتنج|طقم 4 كوسترات راتنج ملوّنة مصنوعة يدوياً بتصاميم مرمرية أنيقة.|145",
                    "مرجعة كتاب راتنج|علامة كتاب شفافة بزهور مجففة مطمورة في راتنج كريستالي شفاف.|35",
                    "كيرينج راتنج ملوّن|حامل مفاتيح صغير مصنوع من الراتنج بألوان جذابة وتصاميم فريدة.|30",
                    "صينية راتنج زخرفية|صينية راتنج شفافة مع نقوش ذهبية مناسبة لتنظيم الإكسسوارات.|195",
                    "لوحة راتنج للحائط|لوحة فنية راتنج مجردة ملوّنة بألوان متناسقة لتزيين الجدران.|275"
                }
            },
            {
                "فخار وسيراميك",
                new[]
                {
                    "كوب خزف يدوي|كوب قهوة سيراميك مصنوع على دولاب الفخار بتزجيج فريد وفردي.|115",
                    "مزهرية سيراميك|مزهرية مزجّجة مصنوعة يدوياً بأشكال عضوية وتزجيج لامع.|185",
                    "طقم أطباق خزف|طقم 4 أطباق سيراميك مصنوعة يدوياً بنقوش هندسية لكل طبق.|380",
                    "أصيص طيني مزخرف|أصيص طيني مصنوع يدوياً بزخارف ملوّنة مناسب للنباتات الداخلية.|95",
                    "طاسة تقديم خزف|طاسة تقديم واسعة مصنوعة يدوياً بتصميم فريد وزخرفة تقليدية.|145"
                }
            },
            {
                "أطقم حرفية",
                new[]
                {
                    "طقم تعليم الكروشيه|طقم مبتدئين شامل لتعلم الكروشيه: خطاطيف، خيوط، دليل خطوات بالصور.|120",
                    "طقم صناعة الصابون|طقم DIY لصنع الصابون الطبيعي في المنزل مع جميع المواد والعطور.|145",
                    "طقم فن الراتنج|طقم راتنج للمبتدئين مع أدوات، أصباغ، وقوالب متنوعة وسهلة الاستخدام.|165",
                    "طقم صنع المجوهرات|طقم متكامل لصنع الحلي: أدوات، أسلاك، خرز، ومشابك متنوعة.|135",
                    "طقم التطريز الكامل|طقم تطريز شامل: إطار، إبر، خيوط DMC، وقماش مع نقشة مطبوعة.|110"
                }
            }
        };

        // ── Variants ────────────────────────────────────────────────
        // Format per product-type: رح نحدد variants مناسبة لكل فئة

        private static readonly Dictionary<string, string[]> CategoryVariants = new()
        {
            { "منتجات الكروشيه", new[] { "صغير", "وسط", "كبير" } },
            { "ملابس تريكو",     new[] { "صغير", "وسط", "كبير", "كبير جداً" } },
            { "حقائب يدوية",     new[] { "صغير", "وسط", "كبير" } },
            { "ديكور منزلي",     new[] { "صغير 30×30 سم", "وسط 50×50 سم", "كبير 70×70 سم" } },
            { "شموع",            new[] { "100غ", "200غ", "350غ" } },
            { "صابون طبيعي",     new[] { "قطعة واحدة 100غ", "3 قطع", "6 قطع" } },
            { "مجوهرات",         new[] { "قياس 16 سم", "قياس 18 سم", "قياس 20 سم" } },
            { "منتجات راتنج",    new[] { "صغير", "وسط", "كبير" } },
            { "فخار وسيراميك",   new[] { "250مل", "350مل", "500مل" } },
            { "أطقم حرفية",      new[] { "مبتدئ", "متوسط", "متقدم" } }
        };

        private static readonly string[] Colors = new[]
        {
            "أبيض", "بيج", "وردي", "أزرق سماوي", "أخضر", "برتقالي", "بنفسجي",
            "أحمر طوبي", "رمادي فاتح", "أزرق داكن", "زيتوني", "لبني"
        };

        // ════════════════════════════════════════════════════════════
        //  روابط صور حقيقية وفعّالة (real & working) لكل فئة
        //  بنستخدم خدمة LoremFlickr (https://loremflickr.com) اللي بترجع
        //  صورة حقيقية مطابقة لكلمة مفتاحية (keyword) من Flickr، والرابط
        //  دايماً شغّال ومباشر (مش لازم نرفع صور بنفسنا).
        //  Format: https://loremflickr.com/{width}/{height}/{keywords}?lock={seed}
        //  - keywords: مفصولة بفاصلة (OR matching) — فضلت بالإنجليزي عمداً لأن
        //    بحث الصور بيشتغل أحسن وأدق بالكلمات الإنجليزية (مش جزء من بيانات
        //    المنتج المعروضة، فقط معامل تقني لجلب الصورة)
        // ════════════════════════════════════════════════════════════
        private static readonly Dictionary<string, string[]> CategoryImageKeywords = new()
        {
            { "منتجات الكروشيه", new[] { "crochet,amigurumi", "crochet,yarn" } },
            { "ملابس تريكو",     new[] { "knitting,wool", "knitted,sweater" } },
            { "حقائب يدوية",     new[] { "leather,bag", "handbag,handmade" } },
            { "ديكور منزلي",     new[] { "macrame,wall", "homedecor,handmade" } },
            { "شموع",            new[] { "candle,soy", "candle,aromatherapy" } },
            { "صابون طبيعي",     new[] { "soap,natural", "soap,handmade" } },
            { "مجوهرات",         new[] { "jewelry,handmade", "bracelet,beads" } },
            { "منتجات راتنج",    new[] { "resin,art", "resin,craft" } },
            { "فخار وسيراميك",   new[] { "pottery,ceramic", "ceramic,mug" } },
            { "أطقم حرفية",      new[] { "diy,craft", "craft,kit" } }
        };

        private static readonly string[] DefaultImageKeywords = { "handmade", "craft" };

        /// <summary>
        /// بناء رابط صورة حقيقي وفعّال من LoremFlickr مطابق لفئة المنتج.
        /// </summary>
        private static string BuildImageUrl(string keywords, int seed, int width = 640, int height = 640)
            => $"https://loremflickr.com/{width}/{height}/{keywords}?lock={seed}";

        public static async Task SeedAsync(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            IConfiguration configuration)
        {
            // ── المتطلبات الأساسية ──────────────────────────────────
            var sellerProfiles = await context.SellerProfiles
                .Include(sp => sp.User)
                .Where(sp => sp.Status == VerificationStatus.Approved)
                .ToListAsync();

            if (sellerProfiles.Count == 0) return;

            var categories = await context.ProductCategories.ToListAsync();
            if (categories.Count == 0) return;

            // تجاهل إذا في منتجات موجودة بالفعل
            if (await context.Products.AnyAsync()) return;

            // ── Seed Products ─────────────────────────────────────────
            int sellerIndex = 0;
            int productCount = 0;
            int imageSeed = 1; // seed لرابط LoremFlickr — يضمن صورة ثابتة ومختلفة لكل منتج

            foreach (var category in categories)
            {
                if (!ProductsData.TryGetValue(category.Name, out var productsInCategory))
                    continue;

                // اختيار variants مناسبة للفئة
                var variants = CategoryVariants.TryGetValue(category.Name, out var cv)
                    ? cv
                    : new[] { "نوع 1", "نوع 2", "نوع 3" };

                foreach (var productData in productsInCategory)
                {
                    var parts = productData.Split('|');
                    var name = parts[0];
                    var description = parts[1];
                    var basePrice = decimal.Parse(parts[2]);

                    // تجنب التكرار
                    if (await context.Products.AnyAsync(p => p.Name == name))
                        continue;

                    var seller = sellerProfiles[sellerIndex % sellerProfiles.Count];

                    // ── Product ──
                    var product = new Product
                    {
                        SellerProfileId = seller.Id,
                        ProductCategoryId = category.Id,
                        Name = name,
                        Description = description,
                        IsPuplished = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-(productCount * 3))
                    };
                    context.Products.Add(product);
                    await context.SaveChangesAsync();

                    // ── ProductVariants ──
                    // كل منتج عنده 2-4 variants حسب نوعه
                    var variantCount = 2 + (productCount % (variants.Length - 1));
                    variantCount = Math.Min(variantCount, variants.Length);

                    for (int v = 0; v < variantCount; v++)
                    {
                        var color = Colors[(Math.Abs(productCount + v) * 3) % Colors.Length];
                        var priceAdj = basePrice + (v * basePrice * 0.15m); // كل variant أغلى بـ 15%
                        var stock = 10 + (Math.Abs(productCount - v) % 40) + 10;

                        context.ProductVariants.Add(new ProductVariant
                        {
                            ProductId = product.Id,
                            Name = $"{variants[v]} – {color}",
                            Price = Math.Round(priceAdj, 2),
                            StockQuantity = stock
                        });
                    }
                    await context.SaveChangesAsync();

                    // ── ProductImages ──
                    // صورة رئيسية + صورة ثانوية لكل منتج — روابط حقيقية وفعّالة (LoremFlickr)
                    var imageKeywords = CategoryImageKeywords.TryGetValue(category.Name, out var ik)
                        ? ik
                        : DefaultImageKeywords;

                    var mainImageUrl = BuildImageUrl(imageKeywords[0], imageSeed++);
                    var secondImageUrl = BuildImageUrl(
                        imageKeywords.Length > 1 ? imageKeywords[1] : imageKeywords[0],
                        imageSeed++);
                    var thirdImageUrl = BuildImageUrl(
                        imageKeywords.Length > 1 ? imageKeywords[1] : imageKeywords[0],
                        imageSeed++);

                    context.ProductImages.AddRange(
                        new ProductImage
                        {
                            ProductId = product.Id,
                            ImageUrl = mainImageUrl,
                            IsPrimary = true,
                            OrderIndex = 1
                        },
                        new ProductImage
                        {
                            ProductId = product.Id,
                            ImageUrl = secondImageUrl,
                            IsPrimary = false,
                            OrderIndex = 2
                        },
                        new ProductImage
                        {
                            ProductId = product.Id,
                            ImageUrl = thirdImageUrl,
                            IsPrimary = false,
                            OrderIndex = 3
                        }
                    );
                    await context.SaveChangesAsync();

                    sellerIndex++;
                    productCount++;
                }
            }

            // ── Seed Orders + Payments ────────────────────────────────
            var traineeProfiles = await context.TraineeProfiles
                .Include(tp => tp.User)
                .ToListAsync();

            if (traineeProfiles.Count == 0) return;

            var allProducts = await context.Products
                .Include(p => p.ProductVariants)
                .Where(p => p.IsPuplished)
                .ToListAsync();

            var orderStatuses = new[]
            {
                OrderStatus.Pending,
                OrderStatus.Processing,
                OrderStatus.Shipped,
                OrderStatus.Delivered
            };

            foreach (var trainee in traineeProfiles)
            {
                var user = await userManager.FindByIdAsync(trainee.UserId.ToString());
                if (user == null) continue;

                // 2-3 طلبات لكل متدرب
                int orderCount = 2 + (Math.Abs(trainee.Id.GetHashCode()) % 2);

                for (int o = 0; o < orderCount; o++)
                {
                    // تجنب إنشاء أكثر من طلب في نفس اليوم لنفس المستخدم
                    var offsetDays = (o + 1) * 7;
                    var orderDate = DateTime.UtcNow.AddDays(-offsetDays);

                    if (await context.Orders.AnyAsync(ord =>
                        ord.UserId == user.Id &&
                        ord.CreatedAt.Date == orderDate.Date))
                        continue;

                    var status = orderStatuses[o % orderStatuses.Length];

                    var order = new Order
                    {
                        UserId = user.Id,
                        Status = status,
                        TotalAmount = 0m,
                        CreatedAt = orderDate
                    };
                    context.Orders.Add(order);
                    await context.SaveChangesAsync();

                    // 1-3 منتجات لكل طلب
                    var itemCount = 1 + (Math.Abs(trainee.Id.GetHashCode() + o) % 3);
                    var chosenProducts = allProducts
                        .OrderBy(_ => Guid.NewGuid())
                        .Take(itemCount)
                        .ToList();

                    decimal total = 0m;
                    foreach (var prod in chosenProducts)
                    {
                        var variant = prod.ProductVariants.FirstOrDefault();
                        if (variant == null) continue;

                        var qty = 1 + (Math.Abs(o) % 3);
                        var unitPrice = variant.Price;
                        total += unitPrice * qty;

                        context.OrderItems.Add(new OrderItem
                        {
                            OrderId = order.Id,
                            ProductVariantId = variant.Id,
                            Quantity = qty,
                            UnitPrice = unitPrice
                        });
                    }

                    order.TotalAmount = total;

                    // Payment للطلبات المعالجة/المشحونة/المُسلَّمة
                    if (status is OrderStatus.Processing
                               or OrderStatus.Shipped
                               or OrderStatus.Delivered)
                    {
                        context.Payments.Add(new Payment
                        {
                            UserId = user.Id,
                            OrderId = order.Id,
                            Amount = total,
                            PaymentMethod = o % 2 == 0 ? "بطاقة ائتمان" : "محفظة إلكترونية",
                            PaymentStatus = "Completed",
                            TransactionReference = $"TXN-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                            PaidAt = orderDate.AddHours(1)
                        });
                    }

                    await context.SaveChangesAsync();
                }
            }
        }
    }
}