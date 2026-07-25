using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Femora.Domain.Entities.LMS;
using Femora.Domain.Entities.LMS.Quizzes;
using Femora.Domain.Entities.Identity;
using Femora.Infrastructure.Data;

namespace Femora.Infrastructure.Persistence.Seeders
{
    internal static class EnrollmentSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            // Any user without a TraineeProfile yet (sellers/instructors/admins in the
            // seed data) gets a minimal one created here, so literally every seeded user
            // ends up enrollable - not just users explicitly tagged "Trainee" in ProfileSeeder.
            var usersWithoutTraineeProfile = await context.Users
                // Exclude admin user from automatic trainee profile creation
                .Where(u => u.Email.EndsWith("@test.com") && !u.Email.Equals("admin@test.com") && !context.TraineeProfiles.Any(tp => tp.UserId == u.Id))
                .ToListAsync();

            foreach (var user in usersWithoutTraineeProfile)
            {
                context.TraineeProfiles.Add(new TraineeProfile
                {
                    UserId = user.Id,
                    SkillLevel = Femora.Domain.Enums.TrainingSkillLevel.Beginner
                });
            }

            if (usersWithoutTraineeProfile.Count > 0)
            {
                await context.SaveChangesAsync();
            }

            // Get all trainee profiles for seeded test users
            var traineeProfiles = await context.TraineeProfiles
                .Include(tp => tp.User)
                // Ensure admin (if present) is not included
                .Where(tp => tp.User.Email.EndsWith("@test.com") && !tp.User.Email.Equals("admin@test.com"))
                .ToListAsync();

            if (traineeProfiles.Count == 0)
                return;

            // Get all courses
            var courses = await context.Courses
                .Include(c => c.Modules)
                .ThenInclude(m => m.Lessons)
                .ToListAsync();

            if (courses.Count == 0)
                return;

            // Collected here instead of thrown immediately, so one bad enrollment can't
            // abort the whole seeding run (which previously meant EVERY remaining user's
            // enrollments silently never got created/saved, since DbContextSeed.SeedAsync
            // has no per-seeder try/catch and just lets exceptions propagate).
            var errors = new List<string>();

            // Each trainee should be enrolled in 3-5 courses
            foreach (var traineeProfile in traineeProfiles)
            {
                var coursesToEnroll = courses
                    .OrderBy(x => Guid.NewGuid())
                    .Take(3 + (Math.Abs(traineeProfile.Id.GetHashCode()) % 3))
                    .ToList();

                foreach (var course in coursesToEnroll)
                {
                    try
                    {
                        await EnrollOneCourseAsync(context, traineeProfile, course);
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"[EnrollmentSeeder] Trainee {traineeProfile.Id} / Course \"{course.Title}\" ({course.Id}): {ex.GetType().Name}: {ex.Message}");
                        // The failed SaveChangesAsync may have left tracked entities in a bad
                        // state for this DbContext - detach everything not yet saved so the
                        // NEXT course/trainee starts clean instead of re-throwing the same error.
                        foreach (var entry in context.ChangeTracker.Entries().Where(e => e.State != EntityState.Unchanged).ToList())
                        {
                            entry.State = EntityState.Detached;
                        }
                    }
                }
            }

            if (errors.Count > 0)
            {
                Console.WriteLine($"[EnrollmentSeeder] Finished with {errors.Count} error(s) (other enrollments were still created):");
                foreach (var error in errors)
                {
                    Console.WriteLine("  - " + error);
                }
            }
        }

        private static async Task EnrollOneCourseAsync(AppDbContext context, TraineeProfile traineeProfile, Course course)
        {
                    // Check if enrollment already exists
                    if (context.Enrollments.Any(e =>
                        e.TraineeProfileId == traineeProfile.Id &&
                        e.CourseId == course.Id))
                        return;

                    // Create enrollment
                    var enrollmentSeed = Math.Abs(HashCode.Combine(traineeProfile.Id, course.Id));
                    var enrollment = new Enrollment
                    {
                        TraineeProfileId = traineeProfile.Id,
                        CourseId = course.Id,
                        PricePaid = course.Price,
                        EnrolledAt = DateTime.UtcNow.AddDays(-(enrollmentSeed % 30)),
                        IsCompleted = false,
                        CompletedAt = null
                    };
                    context.Enrollments.Add(enrollment);
                    await context.SaveChangesAsync();

                    // Add lesson progress + module unlock state.
                    // Progress is modeled cumulatively across the WHOLE course (not reset per
                    // module), matching how a trainee actually studies: lessons are worked
                    // through in order, module by module. Every lesson gets a LessonProgress
                    // row (even ones not reached yet) so completing any lesson never 404s.
                    var modules = course.Modules.OrderBy(m => m.OrderIndex).ToList();
                    var totalLessonsInCourse = modules.Sum(m => m.Lessons.Count);
                    // Reuses enrollmentSeed (trainee+course combined) from above, so every
                    // enrollment gets its own progress instead of the same number repeating
                    // across every course for a given trainee.
                    int completedLessonsCount = totalLessonsInCourse == 0
                        ? 0
                        : 1 + (enrollmentSeed % totalLessonsInCourse);

                    int lessonsSeenSoFar = 0;
                    var moduleFullyCompleted = new Dictionary<Guid, bool>();

                    foreach (var module in modules)
                    {
                        var lessons = module.Lessons.OrderBy(l => l.OrderIndex).ToList();
                        var lessonsCompletedInThisModule = 0;

                        foreach (var lesson in lessons)
                        {
                            var isCompleted = lessonsSeenSoFar < completedLessonsCount;
                            if (isCompleted) lessonsCompletedInThisModule++;
                            lessonsSeenSoFar++;

                            if (!context.LessonProgresses.Any(lp =>
                                lp.EnrollmentId == enrollment.Id &&
                                lp.LessonId == lesson.Id))
                            {
                                var lessonProgress = new LessonProgress
                                {
                                    EnrollmentId = enrollment.Id,
                                    LessonId = lesson.Id,
                                    IsCompleted = isCompleted,
                                    WatchedSeconds = isCompleted && lesson.DurationSeconds.HasValue
                                        ? lesson.DurationSeconds.Value
                                        : (lesson.DurationSeconds.HasValue ? (int)(lesson.DurationSeconds.Value * 0.75) : 0),
                                    LastAccessedAt = isCompleted
                                        ? DateTime.UtcNow.AddHours(-(enrollmentSeed % 24))
                                        : (DateTime?)null
                                };
                                context.LessonProgresses.Add(lessonProgress);
                            }
                        }

                        moduleFullyCompleted[module.Id] = lessons.Count > 0 && lessonsCompletedInThisModule == lessons.Count;
                    }
                    await context.SaveChangesAsync();

                    // Unlock modules sequentially: the first module is always open; each later
                    // module unlocks only once every lesson in the module before it is complete -
                    // mirroring EnrollCommandHandler / unlock-next-module's real behavior, so
                    // seeded data never shows an in-progress or already-finished module as locked.
                    var previousModuleCompleted = true;
                    foreach (var module in modules)
                    {
                        if (!context.EnrollmentModules.Any(em =>
                            em.EnrollmentId == enrollment.Id && em.ModuleId == module.Id))
                        {
                            var enrollmentModule = new EnrollmentModule
                            {
                                EnrollmentId = enrollment.Id,
                                ModuleId = module.Id,
                                IsUnlocked = previousModuleCompleted,
                                UnlockedAt = previousModuleCompleted ? DateTime.UtcNow : (DateTime?)null,
                            };
                            context.EnrollmentModules.Add(enrollmentModule);
                        }

                        previousModuleCompleted = moduleFullyCompleted.GetValueOrDefault(module.Id, false);
                    }
                    await context.SaveChangesAsync();

                    // Add quiz attempts with varied results, and re-derive module unlock
                    // state from ACTUAL quiz-pass status (matching UnlockNextModuleCommandHandler),
                    // not just from lesson-completion like the first pass above did. A module
                    // whose predecessor's quiz wasn't passed must never show as unlocked, even
                    // if every lesson in it was already "watched" by the seed data.
                    var quizzesByModule = await context.Quizzes
                        .Include(q => q.Questions)
                        .ThenInclude(q => q.Choices)
                        .Where(q => q.CourseId == course.Id)
                        .ToListAsync();

                    var moduleQuizPassed = new Dictionary<Guid, bool>();

                    foreach (var module in modules)
                    {
                        var quiz = quizzesByModule.FirstOrDefault(q => q.ModuleId == module.Id);

                        if (quiz == null || !quiz.Questions.Any())
                        {
                            // No quiz for this module - unlocking the next one depends only
                            // on lesson completion, same as CompleteLessonCommandHandler.
                            moduleQuizPassed[module.Id] = true;
                            continue;
                        }

                        // Determine quiz state: Passed, Failed, or Not Attempted
                        var quizState = Math.Abs(traineeProfile.Id.GetHashCode() ^ module.Id.GetHashCode()) % 3;

                        if (quizState == 0) // Not Attempted
                        {
                            moduleQuizPassed[module.Id] = false;
                        }
                        else if (quizState == 1) // Passed
                        {
                            await CreateQuizAttempt(context, quiz, enrollment, traineeProfile, true);
                            moduleQuizPassed[module.Id] = true;
                        }
                        else // Failed
                        {
                            await CreateQuizAttempt(context, quiz, enrollment, traineeProfile, false);
                            moduleQuizPassed[module.Id] = false;
                        }
                    }

                    var unlockGate = true;
                    foreach (var module in modules)
                    {
                        var enrollmentModule = await context.EnrollmentModules
                            .FirstOrDefaultAsync(em => em.EnrollmentId == enrollment.Id && em.ModuleId == module.Id);

                        if (enrollmentModule != null)
                        {
                            enrollmentModule.IsUnlocked = unlockGate;
                            enrollmentModule.UnlockedAt = unlockGate ? (enrollmentModule.UnlockedAt ?? DateTime.UtcNow) : null;
                        }

                        // Next module unlocks only if THIS module's lessons are done AND
                        // (it has no quiz OR its quiz was passed).
                        unlockGate = unlockGate
                            && moduleFullyCompleted.GetValueOrDefault(module.Id, false)
                            && moduleQuizPassed.GetValueOrDefault(module.Id, true);
                    }
                    await context.SaveChangesAsync();
        }

        private static async Task CreateQuizAttempt(AppDbContext context, Quiz quiz, Enrollment enrollment,
            TraineeProfile traineeProfile, bool shouldPass)
        {
            var quizAttemptSeed = Math.Abs(HashCode.Combine(traineeProfile.Id, quiz.Id));
            var quizAttempt = new QuizAttempt
            {
                QuizId = quiz.Id,
                EnrollmentId = enrollment.Id,
                TraineeProfileId = traineeProfile.Id,
                MaxScore = 100,
                Score = shouldPass ? 75 + (quizAttemptSeed % 25) : 30 + (quizAttemptSeed % 30),
                IsPassed = shouldPass,
                AttemptNumber = 1,
                AttemptedAt = DateTime.UtcNow.AddDays(-(quizAttemptSeed % 7)),
                SubmittedAt = DateTime.UtcNow.AddDays(-(quizAttemptSeed % 7))
            };
            context.QuizAttempts.Add(quizAttempt);
            await context.SaveChangesAsync();

            // Add answers for each question
            var questions = quiz.Questions.OrderBy(q => q.OrderIndex).ToList();
            foreach (var question in questions)
            {
                var choices = question.Choices.OrderBy(c => c.Order).ToList();
                var chosenChoice = choices[Math.Abs(traineeProfile.Id.GetHashCode() + question.OrderIndex) % choices.Count];

                var answer = new QuizAttemptAnswer
                {
                    QuizAttemptId = quizAttempt.Id,
                    QuestionId = question.Id,
                    ChoiceId = chosenChoice.Id,
                    IsCorrect = chosenChoice.IsCorrect
                };
                context.QuizAttemptAnswers.Add(answer);
            }
            await context.SaveChangesAsync();
        }
    }
}