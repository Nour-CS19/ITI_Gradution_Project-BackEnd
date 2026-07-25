using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Femora.Domain.Entities.AI;
using Femora.Domain.Entities.Identity;
using Femora.Domain.Enums;
using Femora.Infrastructure.Data;

namespace Femora.Infrastructure.Persistence.Seeders
{
    internal static class AISeeder
    {
        private static readonly Dictionary<string, string[]> ConversationMessages = new()
        {
            {
                "CourseSupport",
                new[]
                {
                    "أنا مهتم أتعلم الكروشيه. أبدأ منين؟",
                    "أنصحك تبدأ بكورس 'الكروشيه للمبتدئين'. بيغطي كل التقنيات الأساسية.",
                    "هحتاج أدوات إيه بالظبط؟",
                    "هتحتاج خطاف كروشيه وخيوط ومقص. عندنا طقم مبتدئين متكامل متاح."
                }
            },
            {
                "MarketplaceSupport",
                new[]
                {
                    "ممكن تقترح عليّ منتجات يدوية حلوة؟",
                    "بناءً على اهتمامك بالكروشيه، أنصحك بدباديب الكروشيه وبطانيات الأطفال عندنا.",
                    "عندكم مجوهرات يدوية؟",
                    "أيوه! عندنا أساور وعقود وحلقان يدوية جميلة مصنوعة من أحجار شبه كريمة."
                }
            },
            {
                "LearningPathPlanning",
                new[]
                {
                    "عايز أبقى محترف في الحرف اليدوية. تنصحني بمسار تعليمي إزاي؟",
                    "أقترح تبدأ بـ 'أساسيات الكروشيه'، بعدين 'أنماط متقدمة'، وأخيرًا 'تقنيات احترافية'.",
                    "هياخد وقت قد إيه؟",
                    "معظم المتدربين بيخلّصوا المسار ده في 3-4 شهور بالممارسة المستمرة."
                }
            }
        };

        private static readonly string[] RecommationReasons = new[]
        {
            "بناءً على تصفحك الأخير",
            "الأكثر رواجًا في فئة اهتمامك",
            "موصى به من مستخدمين مشابهين لك",
            "من الأكثر تداولًا في الحرف اليدوية",
            "يناسب مستوى مهارتك"
        };

        public static async Task SeedAsync(AppDbContext context)
        {
            // Get trainee profiles to create conversations for
            var traineeProfiles = await context.TraineeProfiles
                .Include(tp => tp.User)
                .Take(6)
                .ToListAsync();

            if (traineeProfiles.Count == 0)
                return;

            // Get courses and products for recommendations
            var courses = await context.Courses
                .Where(c => c.IsPublished)
                .OrderBy(c => c.CreatedAt)
                .Take(10)
                .ToListAsync();

            var products = await context.Products
                .Where(p => p.IsPuplished)
                .OrderBy(p => p.CreatedAt)
                .Take(10)
                .ToListAsync();

            int conversationCount = 0;
            int messageCount = 0;
            int recommendationCount = 0;

            // Create conversations for each trainee
            foreach (var traineeProfile in traineeProfiles)
            {
                var contexts = new[] 
                { 
                    AIConversationContext.CourseSupport, 
                    AIConversationContext.MarketplaceSupport, 
                    AIConversationContext.LearningPathPlanning 
                };

                foreach (var convContext in contexts)
                {
                    // Check if conversation already exists
                    if (context.AIConversations.Any(c => 
                        c.UserId == traineeProfile.UserId && 
                        c.Context == convContext))
                        continue;

                    var contextLabel = convContext switch
                    {
                        AIConversationContext.CourseSupport => "الدعم الفني للكورسات",
                        AIConversationContext.MarketplaceSupport => "الدعم الفني للمتجر",
                        AIConversationContext.LearningPathPlanning => "تخطيط المسار التعليمي",
                        _ => convContext.ToString()
                    };

                    var conversation = new AIConversation
                    {
                        UserId = traineeProfile.UserId,
                        Title = $"محادثة حول {contextLabel}",
                        Context = convContext,
                        CreatedAt = DateTime.UtcNow.AddDays(-(traineeProfile.Id.GetHashCode() % 7)),
                        UpdatedAt = DateTime.UtcNow.AddDays(-(traineeProfile.Id.GetHashCode() % 7))
                    };
                    context.AIConversations.Add(conversation);
                    await context.SaveChangesAsync();

                    // Add messages to conversation
                    if (ConversationMessages.TryGetValue(convContext.ToString(), out var messages))
                    {
                        bool isUser = true;
                        foreach (var messageText in messages)
                        {
                            var message = new AIMessage
                            {
                                ConversationId = conversation.Id,
                                Role = isUser ? AIMessageRole.User : AIMessageRole.Assistant,
                                Content = messageText,
                                SentAt = DateTime.UtcNow.AddDays(-(traineeProfile.Id.GetHashCode() % 5))
                                    .AddHours(messageCount++)
                            };
                            context.AIMessages.Add(message);
                            isUser = !isUser;
                        }
                        await context.SaveChangesAsync();
                    }

                    conversationCount++;
                }
            }

            // Create recommendations
            var recommendationTypes = new[] 
            { 
                AIRecommendationType.Course, 
                AIRecommendationType.Product,
                AIRecommendationType.LearningPath
            };

            for (int r = 0; r < 20; r++)
            {
                var trainee = traineeProfiles[r % traineeProfiles.Count];
                var recType = recommendationTypes[r % recommendationTypes.Length];

                Guid entityId;
                string entityType;

                if (recType == AIRecommendationType.Course && courses.Count > 0)
                {
                    var course = courses[r % courses.Count];
                    entityId = course.Id;
                    entityType = "Course";
                }
                else if (recType == AIRecommendationType.Product && products.Count > 0)
                {
                    var product = products[r % products.Count];
                    entityId = product.Id;
                    entityType = "Product";
                }
                else
                {
                    var course = courses[r % courses.Count];
                    entityId = course.Id;
                    entityType = "LearningPath";
                }

                // Check if recommendation already exists
                if (context.AIRecommendations.Any(ar =>
                    ar.UserId == trainee.UserId &&
                    ar.EntityId == entityId &&
                    ar.EntityType == entityType))
                    continue;

                var recommendation = new AIRecommendation
                {
                    UserId = trainee.UserId,
                    Type = recType,
                    EntityId = entityId,
                    EntityType = entityType,
                    IsViewed = (r % 3) == 0,
                    GeneratedAt = DateTime.UtcNow.AddDays(-(trainee.Id.GetHashCode() % 10)),
                    ReasonJson = $"{{\"reason\": \"{RecommationReasons[r % RecommationReasons.Length]}\"}}",
                    Score = 0.7 + ((r % 30) * 0.01)
                };
                context.AIRecommendations.Add(recommendation);
                recommendationCount++;
            }

            await context.SaveChangesAsync();
        }
    }
}
