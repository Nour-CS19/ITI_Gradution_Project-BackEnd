using Femora.Domain.Entities;
using Femora.Domain.Entities.Admin;
using Femora.Domain.Entities.AI;
using Femora.Domain.Entities.Identity;
using Femora.Domain.Entities.LMS;
using Femora.Domain.Entities.LMS.Quizzes;
using Femora.Domain.Entities.Marketplace;
using Femora.Domain.Entities.Onboarding;
using Femora.Domain.Entities.Subscription;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
namespace Femora.Application.Common.Interfaces;
public interface IAppDbContext
{
    // Admin
    DbSet<ApprovalRequest> ApprovalRequests { get; }

    // Identity & Profiles + Related
    DbSet<ApplicationUser> ApplicationUsers { get; }
    DbSet<ApplicationRole> ApplicationRoles { get; }

    // Inherited from IdentityDbContext<ApplicationUser, ApplicationRole, Guid> -- exposed
    // here so handlers can batch-join user IDs against roles in one query instead of
    // calling UserManager.GetRolesAsync() once per user (see GetAllUsersQueryHandler).
    DbSet<IdentityUserRole<Guid>> UserRoles { get; }
    DbSet<TraineeProfile> TraineeProfiles { get; }
    DbSet<SellerProfile> SellerProfiles { get; }
    DbSet<InstructorProfile> InstructorProfiles { get; }
    DbSet<InstructorCredential> InstructorCredentials { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<EmailOtp> EmailOtps { get; }
    DbSet<ProfileApplicationRequest> ProfileApplicationRequests { get; }

    // LMS
    DbSet<CourseCategory> CourseCategories { get; }
    DbSet<CourseReview> CourseReviews { get; }
    DbSet<Course> Courses { get; }
    DbSet<Module> Modules { get; }
    DbSet<Lesson> Lessons { get; }
    DbSet<Resource> Resources { get; }
    DbSet<LessonResource> LessonResources { get; }
    DbSet<LessonProgress> LessonProgresses { get; }
    DbSet<Enrollment> Enrollments { get; }
    DbSet<EnrollmentModule> EnrollmentModules { get; }
    DbSet<Assignment> Assignments { get; }
    DbSet<AssignmentSubmission> AssignmentSubmissions { get; }
    DbSet<Certificate> Certificates { get; }
    DbSet<InstructorEarning> InstructorEarnings { get; }
    DbSet<TraineeLearningGoal> LearningGoals { get; }
    DbSet<TraineePreferredCategory> PreferredCategories { get; }
    DbSet<TraineePreferredProductCategory> PreferredProductCategories { get; }

    DbSet<Quiz> Quizzes { get; }
    DbSet<Question> Questions { get; }
    DbSet<QuizAttempt> QuizAttempts { get; }
    DbSet<QuizAttemptAnswer> QuizAttemptAnswers { get; }
    DbSet<Choice> Choices { get; }
    DbSet<QuizRetryGrant> QuizRetryGrants { get; }

    // Marketplace
    DbSet<ProductCategory> ProductCategories { get; }
    DbSet<Product> Products { get; }
    DbSet<ProductVariant> ProductVariants { get; }
    DbSet<ProductImage> ProductImages { get; }
    DbSet<Order> Orders { get; }
    DbSet<OrderItem> OrderItems { get; }
    DbSet<Cart> Carts { get; }
    DbSet<CartItem> CartItems { get; }
    DbSet<Payment> Payments { get; }
    DbSet<SellerEarning> SellerEarnings { get; }

    // AI
    DbSet<AIConversation> AIConversations { get; }
    DbSet<AIRecommendation> AIRecommendations { get; }
    DbSet<AIMessage> AIMessages { get; }

    // Subscription
    DbSet<SubscriptionPlan> SubscriptionPlans { get; }
    DbSet<UserSubscription> UserSubscriptions { get; }

    // Onboarding
    DbSet<OnboardingInterest> OnboardingInterests { get; }
    DbSet<OnboardingGoal> OnboardingGoals { get; }

    // Change Tracker for concurrency handling
    Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker ChangeTracker { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
