using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Femora.Domain.Entities;
using Femora.Infrastructure.Data;

namespace Femora.Infrastructure.Persistence.Seeders
{
    internal static class AdminCleanupSeeder
    {
        // Matches the default admin created by UserSeeder
        private const string AdminEmail = "admin@test.com";

        public static async Task SeedAsync(AppDbContext context, UserManager<Femora.Domain.Entities.ApplicationUser> userManager)
        {
            var admin = await userManager.FindByEmailAsync(AdminEmail);
            if (admin == null)
                return;

            // Remove any trainee profile for admin
            var trainee = await context.TraineeProfiles.FirstOrDefaultAsync(tp => tp.UserId == admin.Id);
            if (trainee != null)
            {
                // Remove related enrollment data for this trainee
                var enrollments = await context.Enrollments.Where(e => e.TraineeProfileId == trainee.Id).ToListAsync();
                if (enrollments.Any())
                {
                    var enrollmentIds = enrollments.Select(e => e.Id).ToList();

                    var lessonProgresses = context.LessonProgresses.Where(lp => enrollmentIds.Contains(lp.EnrollmentId));
                    context.LessonProgresses.RemoveRange(lessonProgresses);

                    var enrollmentModules = context.EnrollmentModules.Where(em => enrollmentIds.Contains(em.EnrollmentId));
                    context.EnrollmentModules.RemoveRange(enrollmentModules);

                    var quizAttempts = context.QuizAttempts.Where(qa => enrollmentIds.Contains(qa.EnrollmentId)).ToList();
                    if (quizAttempts.Any())
                    {
                        var quizAttemptIds = quizAttempts.Select(qa => qa.Id).ToList();
                        var quizAnswers = context.QuizAttemptAnswers.Where(a => quizAttemptIds.Contains(a.QuizAttemptId));
                        context.QuizAttemptAnswers.RemoveRange(quizAnswers);
                        context.QuizAttempts.RemoveRange(quizAttempts);
                    }

                    context.Enrollments.RemoveRange(enrollments);
                }

                context.TraineeProfiles.Remove(trainee);
            }

            // Remove any marketplace orders/payments made by admin
            var orders = await context.Orders.Where(o => o.UserId == admin.Id).ToListAsync();
            if (orders.Any())
            {
                var orderIds = orders.Select(o => o.Id).ToList();

                var orderItems = context.OrderItems.Where(oi => orderIds.Contains(oi.OrderId));
                context.OrderItems.RemoveRange(orderItems);

                var payments = context.Payments.Where(p => (p.OrderId.HasValue && orderIds.Contains(p.OrderId.Value)) || p.UserId == admin.Id);
                context.Payments.RemoveRange(payments);

                context.Orders.RemoveRange(orders);
            }

            await context.SaveChangesAsync();
        }
    }
}
