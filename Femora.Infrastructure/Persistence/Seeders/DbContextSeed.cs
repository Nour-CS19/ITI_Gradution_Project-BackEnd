/*using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Femora.Domain.Entities;
using Femora.Domain.Entities.Identity;
using Femora.Infrastructure.Data;
using Femora.Application.Common.Interfaces.Repositories;

namespace Femora.Infrastructure.Persistence.Seeders;

public static class DbContextSeed
{
    public static async Task SeedAsync(IServiceProvider rootProvider, IConfiguration configuration)
    {
        using var scope = rootProvider.CreateScope();
        var services = scope.ServiceProvider;

        var context = services.GetRequiredService<AppDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var lessonIndexingRepository = services.GetRequiredService<ILessonIndexingRepository>();

        // Apply pending migrations
        try
        {
            await context.Database.MigrateAsync();
        }
        catch
        {
            // ignore migration errors here; caller may handle migration strategy
        }

        // Execute seeders in order
        // 1. Core setup
        await RoleSeeder.SeedAsync(roleManager);
        await UserSeeder.SeedAsync(userManager, configuration);

        // 2. Profile setup
        await ProfileSeeder.SeedAsync(context, userManager, configuration);

        // 2.5 Ensure admin user remains admin-only: remove any trainee profile, enrollments or orders
        await AdminCleanupSeeder.SeedAsync(context, userManager);

        // 3. Categories
        await CourseCategorySeeder.SeedAsync(context);
        await CategorySeeder.SeedAsync(context);

        // 4. LMS content
        await CourseSeeder.SeedAsync(context, userManager, configuration, lessonIndexingRepository);
        await QuizSeeder.SeedAsync(context);

        // 5. LMS engagement
        await EnrollmentSeeder.SeedAsync(context);

        // 6. Marketplace
        await MarketplaceSeeder.SeedAsync(context, userManager, configuration);

        // 7. Subscriptions
        await SubscriptionSeeder.SeedAsync(context, userManager, configuration);

        // 8. AI features
        await AISeeder.SeedAsync(context);
    }
}



*/

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Femora.Domain.Entities;
using Femora.Domain.Entities.Identity;
using Femora.Infrastructure.Data;
using Femora.Application.Common.Interfaces.Repositories;

namespace Femora.Infrastructure.Persistence.Seeders;

public static class DbContextSeed
{
    public static async Task SeedAsync(IServiceProvider rootProvider, IConfiguration configuration)
    {
        using var scope = rootProvider.CreateScope();
        var services = scope.ServiceProvider;

        var context = services.GetRequiredService<AppDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var lessonIndexingRepository = services.GetRequiredService<ILessonIndexingRepository>();

        // Apply pending migrations
        try
        {
            await context.Database.MigrateAsync();
        }
        catch
        {
            // ignore migration errors here; caller may handle migration strategy
        }

        // Execute seeders in order
        // 1. Core setup
        await RoleSeeder.SeedAsync(roleManager);
        await UserSeeder.SeedAsync(userManager, configuration);

        // 2. Profile setup
        await ProfileSeeder.SeedAsync(context, userManager, configuration);

        // 3. Categories
        await CourseCategorySeeder.SeedAsync(context);
        await CategorySeeder.SeedAsync(context);

        // 4. LMS content
        await CourseSeeder.SeedAsync(context, userManager, configuration, lessonIndexingRepository);
        await QuizSeeder.SeedAsync(context);

        // 5. LMS engagement
        await EnrollmentSeeder.SeedAsync(context);

        // 6. Marketplace
        await ProductSeeder.SeedAsync(context, userManager, configuration);

        // 7. Subscriptions
        await SubscriptionSeeder.SeedAsync(context, userManager, configuration);

        // 8. AI features
        await AISeeder.SeedAsync(context);
    }
}
