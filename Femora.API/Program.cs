using Microsoft.OpenApi.Models;
using Femora.API.Middleware;
using Femora.Infrastructure.Persistence.Seeders;
using Microsoft.Extensions.Logging;
using Femora.Application;
using Femora.Application.Common.Settings;
using Femora.Domain.Entities;
using Femora.Domain.Entities.Identity;
using Femora.Infrastructure;
using Femora.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Femora.Infrastructure.Identity.Claims;
using Femora.Application.Features.Identity.Common.Policies;
using Femora.Domain.Enums;
using System.Text.Json.Serialization;
using System.Text.Json;
using Femora.API.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Femora.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddProblemDetails();
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

            // ✅ FIX 1: Add CamelCase JSON so frontend camelCase matches backend records
            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                    // Several handlers return tracked EF entities directly (e.g. GetMyOrdersQueryHandler
                    // returning Order -> OrderItems -> OrderItem.Order). EF's relationship fixup sets
                    // those back-references automatically, which System.Text.Json can't serialize by
                    // default and silently aborts the response mid-write. IgnoreCycles is a safety net
                    // for every endpoint, not just orders.
                    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                });

            builder.Services.AddOpenApi();

            #region DBContext
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
            );
            #endregion

            #region Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Femora API", Version = "v1" });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter your JWT token."
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                c.MapType<Stream>(() => new OpenApiSchema { Type = "string", Format = "binary" });
            });
            #endregion

            #region Dependency Injections
            builder.Services.AddApplicationDependencies()
               .AddInfrastructureDependencies(builder.Configuration);

            builder.Services.Configure<JwtSettings>(
                builder.Configuration.GetSection("JwtSettings"));

            builder.Services.Configure<ClientAppOptions>(
                builder.Configuration.GetSection(ClientAppOptions.SectionName));
            #endregion

            #region Identity
            builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
            }).AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
            #endregion

            #region JWT
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

            var jwt = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwt.SecretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

            builder.Services.AddAuthorization();
            #endregion

            #region Custom Policy
            builder.Services.AddScoped<IAuthorizationHandler, TraineeAuthorizationHandler>();
            builder.Services.AddScoped<IAuthorizationHandler, InstructorAuthorizationHandler>();
            builder.Services.AddScoped<IAuthorizationHandler, SellerAuthorizationHandler>();

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy(Policies.Trainee,
                    policy => policy.Requirements.Add(new TraineeRequirement()));

                options.AddPolicy(Policies.Instructor,
                    policy => policy.Requirements.Add(new InstructorRequirement()));

                options.AddPolicy(Policies.Seller,
                    policy => policy.Requirements.Add(new SellerRequirement()));

                // AI Assistant is hidden from Admin users; any authenticated non-Admin user may access it.
                options.AddPolicy(Policies.NotAdmin,
                    policy => policy.RequireAssertion(ctx =>
                        ctx.User.Identity?.IsAuthenticated == true && !ctx.User.IsInRole("Admin")));
            });
            #endregion

            #region CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("FrontendPolicy", policy =>
                {
                    policy.WithOrigins(
      "http://localhost:4200",
      "https://localhost:4200",

      "https://iti-gradution-project.vercel.app",
      "https://iti-gradution-project-git-master-nour-eddine-mahers-projects.vercel.app",
      "https://iti-gradution-project-6h8vu93qh-nour-eddine-mahers-projects.vercel.app"
  )
  .AllowAnyHeader()
  .AllowAnyMethod()
  .AllowCredentials();
                });
            });
            #endregion

            builder.Services.AddHttpContextAccessor();

            // In-memory cache used e.g. to avoid re-generating Azure Blob SAS URLs
            // (lesson videos/PDFs, product images) on every single request.
            builder.Services.AddMemoryCache();

            #region Response Compression & Output Caching
            // Compresses JSON API responses (gzip/br) before they go over the wire.
            // On text/JSON payloads this typically cuts transfer size (and therefore
            // perceived response time on slower connections) by 60-80% for free.
            builder.Services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.MimeTypes = Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults.MimeTypes
                    .Concat(new[] { "application/json" });
            });

            // Built-in ASP.NET Core Output Cache (no Redis needed to start).
            // Used below on read-heavy, rarely-changing GET endpoints (course/product listings,
            // categories, filter options) so repeat requests are served from memory instead of
            // re-hitting SQL Server every time.
            builder.Services.AddOutputCache(options =>
            {
                options.AddPolicy("Listings", p => p.Expire(TimeSpan.FromSeconds(60)).SetVaryByQuery("*"));
                options.AddPolicy("StaticLookups", p => p.Expire(TimeSpan.FromMinutes(30)));
            });
            #endregion

            var app = builder.Build();

            // ✅ Debug log to verify Stripe WebhookSecret binding at runtime
            var stripeOpts = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<Femora.Infrastructure.Options.StripeOptions>>().Value;
            Console.WriteLine($"[DEBUG] Bound WebhookSecret: '{stripeOpts.WebhookSecret}'");

            // Seed database/runtime data
            if (builder.Configuration["DisableSeeding"] != "true")
            {
                try
                {
                    DbContextSeed.SeedAsync(app.Services, builder.Configuration).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    var logger = app.Services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Expose Swagger UI and JSON so the frontend / devtools can fetch the OpenAPI spec.
            // Keep these enabled during development and available for local debugging.
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                // Use a relative path so the UI fetches the correct JSON from the same origin.
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Femora API v1");
                c.RoutePrefix = "swagger"; // URL: /swagger
            });
            app.MapOpenApi();

            app.UseExceptionHandler();
            app.UseResponseCompression();
            app.UseHttpsRedirection();
            app.UseCors("FrontendPolicy");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseOutputCache();

            app.MapControllers();

            app.Run();
        }
    }
}