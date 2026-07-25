using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Femora.Domain.Entities;
using Femora.Domain.Entities.Identity;

namespace Femora.Infrastructure.Persistence.Seeders
{
    internal static class UserSeeder
    {
        // Default password used for all seeded users
        private const string DefaultPassword = "Femora@123";

        public static async Task SeedAsync(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            var defaultPassword = configuration["SeedData:DefaultPassword"] ?? DefaultPassword;

            // Define all users to seed: 1 Admin, 2 Trainees, 2 Instructors, 2 Sellers, 2 Multi-profile
            var usersToSeed = new[]
            {
                new { Email = "admin@test.com", FirstName = "System", LastName = "Admin", Role = "Admin" },

                // Trainees
                new { Email = "trainee_1@test.com", FirstName = "Amira", LastName = "Hassan", Role = "User" },
                new { Email = "trainee_2@test.com", FirstName = "Sara", LastName = "Mohamed", Role = "User" },

                // Instructors (Trainee + Instructor)
                new { Email = "instructor_1@test.com", FirstName = "Fatima", LastName = "Al-Rashid", Role = "User" },
                new { Email = "instructor_2@test.com", FirstName = "Ahmed", LastName = "Saleh", Role = "User" },

                // Sellers (Trainee + Seller)
                new { Email = "seller_1@test.com", FirstName = "Laila", LastName = "Nour", Role = "User" },
                new { Email = "seller_2@test.com", FirstName = "Omar", LastName = "Karim", Role = "User" },

                // Multi-profile users (Trainee + Instructor + Seller)
                new { Email = "multi_1@test.com", FirstName = "Nora", LastName = "Amin", Role = "User" },
                new { Email = "multi_2@test.com", FirstName = "Karim", LastName = "Zaki", Role = "User" }
            };

            foreach (var userInfo in usersToSeed)
            {
                var existingUser = await userManager.FindByEmailAsync(userInfo.Email);
                if (existingUser == null)
                {
                    var user = new ApplicationUser
                    {
                        UserName = userInfo.Email,
                        Email = userInfo.Email,
                        EmailConfirmed = true,
                        FirstName = userInfo.FirstName,
                        LastName = userInfo.LastName,
                        IsActive = true
                    };

                    var createResult = await userManager.CreateAsync(user, defaultPassword);
                    if (createResult.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, userInfo.Role);
                    }
                }
            }
        }
    }
}