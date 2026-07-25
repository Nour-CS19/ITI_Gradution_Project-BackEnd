using Femora.Application.Common.Interfaces;
using Femora.Domain.Entities;
using Femora.Domain.Entities.Admin;
using Femora.Domain.Entities.AI;
using Femora.Domain.Entities.Identity;
using Femora.Domain.Entities.LMS;
using Femora.Domain.Entities.LMS.Quizzes;
using Femora.Domain.Entities.Marketplace;
using Femora.Domain.Entities.Onboarding;
using Femora.Domain.Entities.Subscription;
using Femora.Infrastructure.Persistence.Seeders;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Reflection;
using Module = Femora.Domain.Entities.LMS.Module;

namespace Femora.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IAppDbContext
{

    // Admin 
    public DbSet<ApprovalRequest> ApprovalRequests => Set<ApprovalRequest>();

    // Identity & Profiles + Related
    public DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();
    public DbSet<ApplicationRole> ApplicationRoles => Set<ApplicationRole>();
    public DbSet<SellerProfile> SellerProfiles => Set<SellerProfile>();
    public DbSet<TraineeProfile> TraineeProfiles => Set<TraineeProfile>();
    public DbSet<InstructorProfile> InstructorProfiles => Set<InstructorProfile>();
    public DbSet<InstructorCredential> InstructorCredentials => Set<InstructorCredential>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<EmailOtp> EmailOtps => Set<EmailOtp>();
    public DbSet<ProfileApplicationRequest> ProfileApplicationRequests => Set<ProfileApplicationRequest>();

    // LMS
    public DbSet<CourseCategory> CourseCategories => Set<CourseCategory>();
    public DbSet<CourseReview> CourseReviews => Set<CourseReview>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Resource> Resources => Set<Resource>();
    public DbSet<LessonResource> LessonResources => Set<LessonResource>();
    public DbSet<LessonProgress> LessonProgresses => Set<LessonProgress>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<EnrollmentModule> EnrollmentModules => Set<EnrollmentModule>();
    public DbSet<Assignment> Assignments => Set<Assignment>();
    public DbSet<AssignmentSubmission> AssignmentSubmissions => Set<AssignmentSubmission>();
    public DbSet<Certificate> Certificates => Set<Certificate>();
    public DbSet<InstructorEarning> InstructorEarnings => Set<InstructorEarning>();
    public DbSet<TraineeLearningGoal> LearningGoals => Set<TraineeLearningGoal>();
    public DbSet<TraineePreferredCategory> PreferredCategories => Set<TraineePreferredCategory>();
    public DbSet<TraineePreferredProductCategory> PreferredProductCategories => Set<TraineePreferredProductCategory>();
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
    public DbSet<QuizAttemptAnswer> QuizAttemptAnswers => Set<QuizAttemptAnswer>();
    public DbSet<Choice> Choices => Set<Choice>();
    public DbSet<QuizRetryGrant> QuizRetryGrants => Set<QuizRetryGrant>();

    // Marketplace
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<SellerEarning> SellerEarnings => Set<SellerEarning>();

    // AI
    public DbSet<AIConversation> AIConversations => Set<AIConversation>();
    public DbSet<AIRecommendation> AIRecommendations => Set<AIRecommendation>();
    public DbSet<AIMessage> AIMessages => Set<AIMessage>();

    // Subscription
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<UserSubscription> UserSubscriptions => Set<UserSubscription>();

    // Onboarding
    public DbSet<OnboardingInterest> OnboardingInterests => Set<OnboardingInterest>();
    public DbSet<OnboardingGoal> OnboardingGoals => Set<OnboardingGoal>();


    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        builder.SeedOnboardingInterests();
        builder.SeedOnboardingGoals();
    }

    public async Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) =>
                                                                                await Database.BeginTransactionAsync(cancellationToken);
}
