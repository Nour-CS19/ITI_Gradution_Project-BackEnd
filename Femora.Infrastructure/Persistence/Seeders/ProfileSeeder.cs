using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Femora.Domain.Entities;
using Femora.Domain.Entities.Identity;
using Femora.Domain.Enums;
using Femora.Infrastructure.Data;

namespace Femora.Infrastructure.Persistence.Seeders
{
    internal static class ProfileSeeder
    {
        public static async Task SeedAsync(AppDbContext context, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            // Define user profiles mapping
            var userProfilesToCreate = new Dictionary<string, List<string>>
            {
                // Trainees: Trainee profile only
                { "trainee_1@test.com", new List<string> { "Trainee" } },
                { "trainee_2@test.com", new List<string> { "Trainee" } },

                // Instructors: Trainee + Instructor
                { "instructor_1@test.com", new List<string> { "Trainee", "Instructor" } },
                { "instructor_2@test.com", new List<string> { "Trainee", "Instructor" } },

                // Sellers: Trainee + Seller
                { "seller_1@test.com", new List<string> { "Trainee", "Seller" } },
                { "seller_2@test.com", new List<string> { "Trainee", "Seller" } },

                // Multi-profile: Trainee + Instructor + Seller
                { "multi_1@test.com", new List<string> { "Trainee", "Instructor", "Seller" } },
                { "multi_2@test.com", new List<string> { "Trainee", "Instructor", "Seller" } }
            };

            // Arabic instructor and seller specializations and bios
            var instructorSpecializations = new[] { "Crochet & Knitting", "Embroidery & Sewing", "Pottery & Ceramics", "Resin Art", "Macramé" };
            var instructorBios = new[]
            {
                "معلمة متخصصة في فنون التريكو والكروشيه التقليدية",
                "خبيرة في الحرف اليدوية والتطريز اليدوي",
                "فنانة متميزة في الخزف والفخار",
                "متخصصة في فن الراتنج والإبوكسي",
                "معلمة الماكرامية والفنون الحبلية"
            };

            var sellerStoreNames = new[]
            {
                "متجر الحرف اليدوية الفاخرة",
                "دكان الفنون التقليدية",
                "محل الإبداع والجمال",
                "متجر الصناعات اليدوية الأصيلة",
                "دكان الحرفيين العرب"
            };

            var sellerStoreDescriptions = new[]
            {
                "نوفر أفضل منتجات الحرف اليدوية الأصيلة والعالية الجودة",
                "متخصصون في المنتجات التقليدية الحقيقية المصنوعة يدويًا",
                "لدينا مجموعة متميزة من الإبداعات الفنية اليدوية",
                "نعرض أجمل الحرف اليدوية من صنع الفنانين المحليين",
                "متجر متخصص في المنتجات الحرفية الأصيلة والفريدة"
            };

            int specIndex = 0;
            int storeIndex = 0;

            foreach (var userEmail in userProfilesToCreate.Keys)
            {
                var user = await userManager.FindByEmailAsync(userEmail);
                if (user == null)
                    continue;

                var profileTypes = userProfilesToCreate[userEmail];

                // Create TraineeProfile if needed
                if (profileTypes.Contains("Trainee"))
                {
                    var existingTraineeProfile = await context.TraineeProfiles
                        .FirstOrDefaultAsync(p => p.UserId == user.Id);

                    if (existingTraineeProfile == null)
                    {
                        var traineeProfile = new TraineeProfile
                        {
                            UserId = user.Id,
                            SkillLevel = TrainingSkillLevel.Beginner
                        };
                        context.TraineeProfiles.Add(traineeProfile);
                    }
                }

                // Create InstructorProfile if needed
                if (profileTypes.Contains("Instructor"))
                {
                    var existingInstructorProfile = await context.InstructorProfiles
                        .FirstOrDefaultAsync(p => p.UserId == user.Id);

                    if (existingInstructorProfile == null)
                    {
                        var instructorProfile = new InstructorProfile
                        {
                            UserId = user.Id,
                            Specialization = instructorSpecializations[specIndex % instructorSpecializations.Length],
                            Bio = instructorBios[specIndex % instructorBios.Length],
                            Rating = 5.0f,
                            TotalEarnings = 0m,
                            Status = VerificationStatus.Approved,
                            VerifiedAt = DateTime.UtcNow
                        };
                        context.InstructorProfiles.Add(instructorProfile);
                        specIndex++;
                    }
                }

                // Create SellerProfile if needed
                if (profileTypes.Contains("Seller"))
                {
                    var existingSellerProfile = await context.SellerProfiles
                        .FirstOrDefaultAsync(p => p.UserId == user.Id);

                    if (existingSellerProfile == null)
                    {
                        var sellerProfile = new SellerProfile
                        {
                            UserId = user.Id,
                            StoreName = sellerStoreNames[storeIndex % sellerStoreNames.Length],
                            StoreDescription = sellerStoreDescriptions[storeIndex % sellerStoreDescriptions.Length],
                            Rating = 4.5f,
                            TotalEarnings = 0m,
                            TaxAmount = 0m,
                            Status = VerificationStatus.Approved,
                            VerifiedAt = DateTime.UtcNow
                        };
                        context.SellerProfiles.Add(sellerProfile);
                        storeIndex++;
                    }
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
