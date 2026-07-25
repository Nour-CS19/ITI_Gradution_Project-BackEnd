using Femora.Application.Common.Interfaces;
using Femora.Application.Common.Interfaces.Repositories;
using Femora.Application.Common.Interfaces.Repositories.LMS;
using Femora.Application.Common.Interfaces.Repositories.MarketPlace;
using Femora.Infrastructure.Data;
using Femora.Infrastructure.Identity.Services;
using Femora.Infrastructure.Options;
using Femora.Infrastructure.Payments;
using Femora.Infrastructure.Repositories;
using Femora.Infrastructure.Repositories.Email;
using Femora.Infrastructure.Repositories.ExternalAuth;
using Femora.Infrastructure.Repositories.LMS;
using Femora.Infrastructure.Repositoies.LMS;
using Femora.Infrastructure.Repositoies.MarketPlace;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Femora.Application.Common.Interfaces.Repositories.Email;
using Femora.Application.Common.Interfaces.Repositories.ExternalAuth;

namespace Femora.Infrastructure;

public static class ModuleInfrastructureDependencies
{
    public static IServiceCollection AddInfrastructureDependencies(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Options ───────────────────────────────────────────────────────────
        services.Configure<AzureBlobStorageOptions>(configuration.GetSection(AzureBlobStorageOptions.SectionName));
        services.Configure<AzureOpenAIOptions>(configuration.GetSection(AzureOpenAIOptions.SectionName));
        services.Configure<AzureSearchOptions>(configuration.GetSection(AzureSearchOptions.SectionName));
        services.Configure<StripeOptions>(configuration.GetSection(StripeOptions.SectionName));
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
        services.Configure<ExternalAuthOptions>(configuration.GetSection(ExternalAuthOptions.SectionName));

        // ── DbContext ─────────────────────────────────────────────────────────
        services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());


        // ── Identity Services ─────────────────────────────────────────────────
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IProfileResolutionService, ProfileResolutionService>();
        services.AddScoped<IProfileActivationService, ProfileActivationService>();
        services.AddScoped<IOnboardingProfileSyncService, OnboardingProfileSyncService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // ── Stripe ────────────────────────────────────────────────────────────
        services.AddScoped<IStripeService, StripeService>();

        // ── Email Repository ──────────────────────────────────────────────────
        services.AddScoped<IEmailRepository, EmailRepository>();

        // ── External Auth Repository ──────────────────────────────────────────
        services.AddHttpClient("Facebook");
        services.AddScoped<IExternalAuthRepository, ExternalAuthRepository>();

        // ── AI Repositories ───────────────────────────────────────────────────
        services.AddSingleton<IBlobStorageRepository, BlobStorageRepository>();
        services.AddSingleton<IEmbeddingRepository, EmbeddingRepository>();
        services.AddSingleton<ISearchIndexRepository, SearchIndexRepository>();
        services.AddSingleton<ITextExtractionRepository, TextExtractionRepository>();
        services.AddSingleton<ITextChunkerRepository, TextChunkerRepository>();
        services.AddSingleton<IAIQuizGeneratorRepository, AiQuizGeneratorRepository>();
        services.AddSingleton<IChatCompletionRepository, ChatCompletionRepository>();
        services.AddSingleton<IPriceSuggestionRepository, PriceSuggestionRepository>();
        services.AddSingleton<ILessonPdfRepository, LessonPdfRepository>();
        services.AddScoped<ILessonIndexingRepository, LessonIndexingRepository>();

        // ── LMS Repositories ──────────────────────────────────────────────────
        services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
        services.AddScoped<ICourseRepository, CourseRepository>();
        services.AddScoped<IQuizRepository, QuizRepository>();
        services.AddScoped<IModuleRepository, ModuleRepository>();
        services.AddScoped<IEnrollmentModuleRepository, EnrollmentModuleRepository>();

        // ── Marketplace Repositories ──────────────────────────────────────────
        services.AddScoped<ICartRepository, CartRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        return services;
    }
}
