using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

using Femora.API;
using Femora.Application.Common.Interfaces;
using Femora.Application.Features.LMS.Courses.Commands;
using Femora.Domain.Entities.Identity;
using Femora.Domain.Entities.LMS;
using Femora.Domain.Enums;
using Femora.Infrastructure.Data;

namespace Femora.Tests
{
    public class CourseControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly Guid _testUserId = Guid.NewGuid();
        private readonly Guid _testInstructorProfileId = Guid.NewGuid();

        public CourseControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.UseSetting("DisableSeeding", "true");
                builder.ConfigureTestServices(services =>
                {
                    var descriptors = services.Where(
                        d => d.ServiceType == typeof(DbContextOptions<AppDbContext>) ||
                             d.ServiceType == typeof(DbContextOptions) ||
                             d.ServiceType == typeof(AppDbContext) ||
                             d.ServiceType == typeof(IAppDbContext)).ToList();
                    
                    foreach (var descriptor in descriptors)
                    {
                        services.Remove(descriptor);
                    }

                    var options = new DbContextOptionsBuilder<AppDbContext>()
                        .UseInMemoryDatabase("InMemoryDbForIntegrationTesting")
                        .Options;

                    services.AddSingleton(options);
                    services.AddScoped(provider => new AppDbContext(provider.GetRequiredService<DbContextOptions<AppDbContext>>()));
                    services.AddScoped<IAppDbContext>(provider => provider.GetRequiredService<AppDbContext>());

                    var userDescriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(ICurrentUserService));
                    if (userDescriptor != null)
                    {
                        services.Remove(userDescriptor);
                    }

                    var mockCurrentUser = new Mock<ICurrentUserService>();
                    mockCurrentUser.Setup(x => x.UserId).Returns(_testUserId);
                    services.AddScoped(_ => mockCurrentUser.Object);

                    services.AddAuthentication("TestScheme")
                        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", options => { });

                    services.Configure<AuthenticationOptions>(options =>
                    {
                        options.DefaultAuthenticateScheme = "TestScheme";
                        options.DefaultChallengeScheme = "TestScheme";
                        options.DefaultForbidScheme = "TestScheme";
                        options.DefaultSignInScheme = "TestScheme";
                        options.DefaultSignOutScheme = "TestScheme";
                    });
                });
            });
        }

        private void EnsureSeeded()
        {
            using var scope = _factory.Server.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();

            if (!db.InstructorProfiles.Any(ip => ip.Id == _testInstructorProfileId))
            {
                db.InstructorProfiles.Add(new InstructorProfile
                {
                    Id = _testInstructorProfileId,
                    UserId = _testUserId
                });
                db.SaveChanges();
            }
        }

        private HttpClient GetInstructorClient(string? customAuthValue = null)
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestScheme", customAuthValue ?? "Instructor");
            return client;
        }

        private HttpClient GetAdminClient()
        {
            var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("TestScheme", "Admin");
            return client;
        }

        private async Task<Guid> SeedCourseWithModulesAndLessons(int moduleCount, int lessonsPerModule, CourseStatus status = CourseStatus.Draft, Guid? customInstructorUserId = null)
        {
            EnsureSeeded();
            using var scope = _factory.Server.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var instructorProfileId = _testInstructorProfileId;
            if (customInstructorUserId.HasValue)
            {
                var customProfile = new InstructorProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = customInstructorUserId.Value
                };
                db.InstructorProfiles.Add(customProfile);
                instructorProfileId = customProfile.Id;
            }

            var course = new Course
            {
                Id = Guid.NewGuid(),
                InstructorProfileId = instructorProfileId,
                Title = $"Test Course {Guid.NewGuid()}",
                Description = "Integration test description",
                Price = 149.99m,
                Category = "Handicrafts",
                Level = CourseLevel.Beginner,
                Language = "Arabic",
                Status = status
            };
            db.Courses.Add(course);

            for (int i = 0; i < moduleCount; i++)
            {
                var module = new Module
                {
                    Id = Guid.NewGuid(),
                    CourseId = course.Id,
                    Title = $"Module {i + 1}",
                    OrderIndex = i + 1
                };
                db.Modules.Add(module);

                for (int j = 0; j < lessonsPerModule; j++)
                {
                    var lesson = new Lesson
                    {
                        Id = Guid.NewGuid(),
                        ModuleId = module.Id,
                        Title = $"Lesson {j + 1}",
                        OrderIndex = j + 1
                    };
                    db.Lessons.Add(lesson);
                }
            }

            await db.SaveChangesAsync();
            return course.Id;
        }

        [Fact]
        public async Task Post_CreateCourse_SetsStatusToDraft()
        {
            // Arrange
            var client = GetInstructorClient();
            EnsureSeeded();
            var payload = new
            {
                instructorProfileId = _testInstructorProfileId.ToString(),
                title = "Draft Course Test",
                description = "Integration test description",
                price = 149.99m,
                category = "Handicrafts",
                level = 1,
                language = "Arabic",
                thumbnailUrl = "https://example.com/cover.jpg"
            };

            // Act
            var response = await client.PostAsJsonAsync("/api/courses", payload);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            // Check db
            using var scope = _factory.Server.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var course = await db.Courses.FirstOrDefaultAsync(c => c.Title == "Draft Course Test");
            Assert.NotNull(course);
            Assert.False(course.IsPublished);
            Assert.False(course.RequiresApproval);
            Assert.Equal(CourseStatus.Draft, course.Status);
        }

        [Fact]
        public async Task Test_G_SubmitCourse_With_Zero_Modules_Returns_400()
        {
            // Arrange
            var client = GetInstructorClient();
            var courseId = await SeedCourseWithModulesAndLessons(0, 0);

            // Act
            var response = await client.PostAsync($"/api/courses/{courseId}/submit", null);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Test_H_SubmitCourse_With_Modules_But_Zero_Lessons_Returns_400()
        {
            // Arrange
            var client = GetInstructorClient();
            var courseId = await SeedCourseWithModulesAndLessons(1, 0);

            // Act
            var response = await client.PostAsync($"/api/courses/{courseId}/submit", null);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task Test_I_Submit_Valid_Course_Sets_Status_To_UnderReview()
        {
            // Arrange
            var client = GetInstructorClient();
            var courseId = await SeedCourseWithModulesAndLessons(1, 1);

            // Act
            var response = await client.PostAsync($"/api/courses/{courseId}/submit", null);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Verify status in DB
            using var scope = _factory.Server.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var course = await db.Courses.FindAsync(courseId);
            Assert.NotNull(course);
            Assert.Equal(CourseStatus.UnderReview, course.Status);
        }

        [Fact]
        public async Task Test_J_Admin_Approves_Course_Sets_Status_To_Published()
        {
            // Arrange
            var instructorClient = GetInstructorClient();
            var courseId = await SeedCourseWithModulesAndLessons(1, 1);

            // Submit for review
            var submitResponse = await instructorClient.PostAsync($"/api/courses/{courseId}/submit", null);
            Assert.Equal(HttpStatusCode.OK, submitResponse.StatusCode);

            // Act: Admin approves
            var adminClient = GetAdminClient();
            var approveResponse = await adminClient.PostAsync($"/api/courses/{courseId}/approve", null);

            // Assert
            Assert.Equal(HttpStatusCode.OK, approveResponse.StatusCode);

            // Verify status in DB
            using var scope = _factory.Server.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var course = await db.Courses.FindAsync(courseId);
            Assert.NotNull(course);
            Assert.Equal(CourseStatus.Published, course.Status);
            Assert.True(course.IsPublished);
        }

        [Fact]
        public async Task Test_K_Admin_Rejects_Course_Sets_Status_To_Rejected_And_Stores_Reason()
        {
            // Arrange
            var instructorClient = GetInstructorClient();
            var courseId = await SeedCourseWithModulesAndLessons(1, 1);

            // Submit for review
            var submitResponse = await instructorClient.PostAsync($"/api/courses/{courseId}/submit", null);
            Assert.Equal(HttpStatusCode.OK, submitResponse.StatusCode);

            // Act: Admin rejects
            var adminClient = GetAdminClient();
            var payload = new { reason = "Incorrect curriculum layout" };
            var rejectResponse = await adminClient.PostAsJsonAsync($"/api/courses/{courseId}/reject", payload);

            // Assert
            Assert.Equal(HttpStatusCode.OK, rejectResponse.StatusCode);

            // Verify status in DB
            using var scope = _factory.Server.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var course = await db.Courses.FindAsync(courseId);
            Assert.NotNull(course);
            Assert.Equal(CourseStatus.Rejected, course.Status);
            Assert.False(course.IsPublished);

            var latestRequest = await db.ApprovalRequests
                .Where(x => x.EntityId == courseId && x.Type == ApprovalEntityType.CourseApproval)
                .OrderByDescending(x => x.RequestedAt)
                .FirstOrDefaultAsync();

            Assert.NotNull(latestRequest);
            Assert.Equal(ApprovalStatus.Rejected, latestRequest.ApprovalStatus);
            Assert.Contains("Incorrect curriculum layout", latestRequest.Note);
        }

        [Fact]
        public async Task Test_L_Instructor_Calls_Approve_Returns_403()
        {
            // Arrange
            var client = GetInstructorClient();
            var courseId = await SeedCourseWithModulesAndLessons(1, 1);

            // Act
            var response = await client.PostAsync($"/api/courses/{courseId}/approve", null);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Test_M_Instructor_Calls_Submit_On_Other_Course_Returns_403()
        {
            // Arrange
            var otherUserId = Guid.NewGuid();
            var client = GetInstructorClient(customAuthValue: $"Instructor_{otherUserId}");
            var courseId = await SeedCourseWithModulesAndLessons(1, 1); // course owned by _testUserId

            // Act
            var response = await client.PostAsync($"/api/courses/{courseId}/submit", null);

            // Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task Test_N_Submit_Already_UnderReview_Course_Returns_400()
        {
            // Arrange
            var client = GetInstructorClient();
            var courseId = await SeedCourseWithModulesAndLessons(1, 1, status: CourseStatus.UnderReview);

            // Act
            var response = await client.PostAsync($"/api/courses/{courseId}/submit", null);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // --- Custom Authentication Handler ---
        private class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
        {
            private readonly ICurrentUserService _currentUserService;

            public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
                ILoggerFactory logger, UrlEncoder encoder, ICurrentUserService currentUserService)
                : base(options, logger, encoder)
            {
                _currentUserService = currentUserService;
            }

            protected override Task<AuthenticateResult> HandleAuthenticateAsync()
            {
                var authHeader = Request.Headers["Authorization"].ToString();
                var role = "Instructor";
                var userId = Guid.NewGuid().ToString();

                if (authHeader.Contains("Admin"))
                {
                    role = "Admin";
                }
                else if (authHeader.Contains("Instructor"))
                {
                    if (authHeader.Contains("Instructor_"))
                    {
                        userId = authHeader.Split('_')[1];
                    }
                    else
                    {
                        userId = _currentUserService.UserId.ToString();
                    }
                }

                var claims = new[] {
                    new Claim(ClaimTypes.Name, $"Test {role}"),
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim(ClaimTypes.Role, role)
                };
                var identity = new ClaimsIdentity(claims, "TestScheme");
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, "TestScheme");

                return Task.FromResult(AuthenticateResult.Success(ticket));
            }
        }
    }
}
