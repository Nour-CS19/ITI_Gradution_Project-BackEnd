using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Femora.Domain.Entities;
using Femora.Domain.Entities.Identity;
using Femora.Domain.Entities.LMS;
using Femora.Domain.Enums;
using Femora.Infrastructure.Data;
using Femora.Application.Common.Interfaces;
using Femora.Application.Features.LMS.Courses.Commands;
using Femora.Application.Features.LMS.Lesson.Queries.GetLessonById;
using Femora.Application.Common.Interfaces.Repositories;
using Femora.Application.Features.LMS.Modules.Commands.CreateModule;

namespace Femora.Tests
{
    public class CourseWorkflowTests
    {
        private AppDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private Mock<UserManager<ApplicationUser>> GetMockUserManager()
        {
            var store = new Mock<IUserStore<ApplicationUser>>();
            return new Mock<UserManager<ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null);
        }

        [Fact]
        public async Task Instructor_Calls_Delete_On_Zero_Enrollment_Course_Throws_Unauthorized()
        {
            // Arrange
            using var context = GetDbContext();
            var mockUserManager = GetMockUserManager();

            var instructorUserId = Guid.NewGuid();
            var instructorUser = new ApplicationUser { Id = instructorUserId, UserName = "instructor" };

            mockUserManager.Setup(m => m.FindByIdAsync(instructorUserId.ToString()))
                .ReturnsAsync(instructorUser);
            mockUserManager.Setup(m => m.IsInRoleAsync(instructorUser, "Admin"))
                .ReturnsAsync(false);

            var courseId = Guid.NewGuid();
            var course = new Course
            {
                Id = courseId,
                Title = "Test Course",
                InstructorProfileId = Guid.NewGuid(),
                Level = CourseLevel.Beginner
            };
            context.Courses.Add(course);
            await context.SaveChangesAsync();

            var handler = new DeleteCourseHandler(context, mockUserManager.Object);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                await handler.Handle(new DeleteCourseCommand(courseId, instructorUserId), CancellationToken.None)
            );
        }

        [Fact]
        public async Task Admin_Calls_Delete_On_Course_With_Enrollments_Throws_InvalidOperation()
        {
            // Arrange
            using var context = GetDbContext();
            var mockUserManager = GetMockUserManager();

            var adminUserId = Guid.NewGuid();
            var adminUser = new ApplicationUser { Id = adminUserId, UserName = "admin" };

            mockUserManager.Setup(m => m.FindByIdAsync(adminUserId.ToString()))
                .ReturnsAsync(adminUser);
            mockUserManager.Setup(m => m.IsInRoleAsync(adminUser, "Admin"))
                .ReturnsAsync(true);

            var courseId = Guid.NewGuid();
            var course = new Course
            {
                Id = courseId,
                Title = "Test Course",
                InstructorProfileId = Guid.NewGuid(),
                Level = CourseLevel.Beginner
            };
            context.Courses.Add(course);

            var enrollment = new Enrollment
            {
                Id = Guid.NewGuid(),
                CourseId = courseId,
                TraineeProfileId = Guid.NewGuid()
            };
            context.Enrollments.Add(enrollment);
            await context.SaveChangesAsync();

            var handler = new DeleteCourseHandler(context, mockUserManager.Object);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await handler.Handle(new DeleteCourseCommand(courseId, adminUserId), CancellationToken.None)
            );
        }

        [Fact]
        public async Task Admin_Calls_Delete_On_Course_With_No_InstructorProfile_Does_Not_Throw_NullRef()
        {
            // Arrange
            using var context = GetDbContext();
            var mockUserManager = GetMockUserManager();

            var adminUserId = Guid.NewGuid();
            var adminUser = new ApplicationUser { Id = adminUserId, UserName = "admin" };

            mockUserManager.Setup(m => m.FindByIdAsync(adminUserId.ToString()))
                .ReturnsAsync(adminUser);
            mockUserManager.Setup(m => m.IsInRoleAsync(adminUser, "Admin"))
                .ReturnsAsync(true);

            var courseId = Guid.NewGuid();
            var course = new Course
            {
                Id = courseId,
                Title = "Test Course",
                InstructorProfileId = Guid.NewGuid(),
                Level = CourseLevel.Beginner
            };
            context.Courses.Add(course);
            await context.SaveChangesAsync();

            var handler = new DeleteCourseHandler(context, mockUserManager.Object);

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                await handler.Handle(new DeleteCourseCommand(courseId, adminUserId), CancellationToken.None)
            );

            // Assert
            Assert.Null(exception);
            var deletedCourse = await context.Courses.FindAsync(courseId);
            Assert.Null(deletedCourse);
        }

        [Fact]
        public async Task Instructor_A_Calls_Archive_On_Course_Owned_By_Instructor_B_Throws_Unauthorized()
        {
            // Arrange
            using var context = GetDbContext();
            var mockUserManager = GetMockUserManager();

            var instructorAUserId = Guid.NewGuid();
            var instructorAUser = new ApplicationUser { Id = instructorAUserId, UserName = "instructorA" };

            var instructorBUserId = Guid.NewGuid();

            mockUserManager.Setup(m => m.FindByIdAsync(instructorAUserId.ToString()))
                .ReturnsAsync(instructorAUser);
            mockUserManager.Setup(m => m.IsInRoleAsync(instructorAUser, "Admin"))
                .ReturnsAsync(false);

            var instructorProfileB = new InstructorProfile
            {
                Id = Guid.NewGuid(),
                UserId = instructorBUserId,
                Bio = "Instructor B bio"
            };
            context.InstructorProfiles.Add(instructorProfileB);

            var courseId = Guid.NewGuid();
            var course = new Course
            {
                Id = courseId,
                Title = "Test Course",
                InstructorProfileId = instructorProfileB.Id,
                Level = CourseLevel.Beginner
            };
            context.Courses.Add(course);
            await context.SaveChangesAsync();

            var handler = new ArchiveCourseHandler(context, mockUserManager.Object);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                await handler.Handle(new ArchiveCourseCommand(courseId, instructorAUserId), CancellationToken.None)
            );
        }

        [Fact]
        public async Task Instructor_A_Calls_Unpublish_On_Course_Owned_By_Instructor_B_Throws_Unauthorized()
        {
            // Arrange
            using var context = GetDbContext();
            var mockUserManager = GetMockUserManager();

            var instructorAUserId = Guid.NewGuid();
            var instructorAUser = new ApplicationUser { Id = instructorAUserId, UserName = "instructorA" };

            var instructorBUserId = Guid.NewGuid();

            mockUserManager.Setup(m => m.FindByIdAsync(instructorAUserId.ToString()))
                .ReturnsAsync(instructorAUser);
            mockUserManager.Setup(m => m.IsInRoleAsync(instructorAUser, "Admin"))
                .ReturnsAsync(false);

            var instructorProfileB = new InstructorProfile
            {
                Id = Guid.NewGuid(),
                UserId = instructorBUserId,
                Bio = "Instructor B bio"
            };
            context.InstructorProfiles.Add(instructorProfileB);

            var courseId = Guid.NewGuid();
            var course = new Course
            {
                Id = courseId,
                Title = "Test Course",
                InstructorProfileId = instructorProfileB.Id,
                Level = CourseLevel.Beginner
            };
            context.Courses.Add(course);
            await context.SaveChangesAsync();

            var handler = new UnpublishCourseHandler(context, mockUserManager.Object);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                await handler.Handle(new UnpublishCourseCommand(courseId, instructorAUserId), CancellationToken.None)
            );
        }

        [Fact]
        public async Task Enrolled_Student_Can_Access_Lesson_After_Course_Archived()
        {
            // Arrange
            using var context = GetDbContext();
            var mockBlobStorage = new Mock<IBlobStorageRepository>();
            mockBlobStorage.Setup(b => b.GetSasUrl(It.IsAny<string>(), It.IsAny<TimeSpan>()))
                .Returns((string path, TimeSpan expiry) => $"https://sas-url.com/{path}");

            var studentUserId = Guid.NewGuid();
            var mockCurrentUser = new Mock<ICurrentUserService>();
            mockCurrentUser.Setup(u => u.UserId).Returns(studentUserId);

            var instructorProfileId = Guid.NewGuid();
            var instructorProfile = new InstructorProfile
            {
                Id = instructorProfileId,
                UserId = Guid.NewGuid(),
                Bio = "Bio"
            };
            context.InstructorProfiles.Add(instructorProfile);

            var courseId = Guid.NewGuid();
            var course = new Course
            {
                Id = courseId,
                Title = "Archived Course",
                InstructorProfileId = instructorProfileId,
                Level = CourseLevel.Beginner,
                IsArchived = true,
                IsPublished = false
            };
            context.Courses.Add(course);

            var traineeProfile = new TraineeProfile
            {
                Id = Guid.NewGuid(),
                UserId = studentUserId
            };
            context.TraineeProfiles.Add(traineeProfile);

            var enrollment = new Enrollment
            {
                Id = Guid.NewGuid(),
                CourseId = courseId,
                TraineeProfileId = traineeProfile.Id
            };
            context.Enrollments.Add(enrollment);

            var moduleId = Guid.NewGuid();
            var module = new Module
            {
                Id = moduleId,
                CourseId = courseId,
                Title = "Module 1"
            };
            context.Modules.Add(module);

            var lessonId = Guid.NewGuid();
            var lesson = new Lesson
            {
                Id = lessonId,
                ModuleId = moduleId,
                Title = "Lesson 1",
                ContentUrl = "lesson-resources/video1.mp4",
                DurationSeconds = 120,
                IsPreview = false
            };
            context.Lessons.Add(lesson);
            await context.SaveChangesAsync();

            var query = new GetLessonByIdQuery(lessonId);
            var handler = new GetLessonByIdHandler(context, mockBlobStorage.Object, mockCurrentUser.Object);

            // Act
            var result = await handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Lesson 1", result.Title);
            Assert.Contains("sas-url.com/lesson-resources/video1.mp4", result.ContentUrl);
        }

        [Fact]
        public async Task User_Not_Enrolled_Cannot_Access_Lesson_Throws_Unauthorized()
        {
            // Arrange
            using var context = GetDbContext();
            var mockBlobStorage = new Mock<IBlobStorageRepository>();
            mockBlobStorage.Setup(b => b.GetSasUrl(It.IsAny<string>(), It.IsAny<TimeSpan>()))
                .Returns((string path, TimeSpan expiry) => $"https://sas-url.com/{path}");

            var nonEnrolledUserId = Guid.NewGuid();
            var mockCurrentUser = new Mock<ICurrentUserService>();
            mockCurrentUser.Setup(u => u.UserId).Returns(nonEnrolledUserId);

            var instructorProfileId = Guid.NewGuid();
            var instructorProfile = new InstructorProfile
            {
                Id = instructorProfileId,
                UserId = Guid.NewGuid(),
                Bio = "Bio"
            };
            context.InstructorProfiles.Add(instructorProfile);

            var courseId = Guid.NewGuid();
            var course = new Course
            {
                Id = courseId,
                Title = "Private Course",
                InstructorProfileId = instructorProfileId,
                Level = CourseLevel.Beginner,
                IsArchived = false,
                IsPublished = true
            };
            context.Courses.Add(course);

            var moduleId = Guid.NewGuid();
            var module = new Module
            {
                Id = moduleId,
                CourseId = courseId,
                Title = "Module 1"
            };
            context.Modules.Add(module);

            var lessonId = Guid.NewGuid();
            var lesson = new Lesson
            {
                Id = lessonId,
                ModuleId = moduleId,
                Title = "Private Paid Lesson",
                ContentUrl = "lesson-resources/video1.mp4",
                DurationSeconds = 120,
                IsPreview = false
            };
            context.Lessons.Add(lesson);
            await context.SaveChangesAsync();

            var query = new GetLessonByIdQuery(lessonId);
            var handler = new GetLessonByIdHandler(context, mockBlobStorage.Object, mockCurrentUser.Object);

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
                await handler.Handle(query, CancellationToken.None)
            );
        }

        [Fact]
        public async Task Instructor_Calls_Archive_On_Orphaned_Course_Does_Not_Throw_NullRef()
        {
            // Arrange
            using var context = GetDbContext();
            var mockUserManager = GetMockUserManager();

            var instructorUserId = Guid.NewGuid();
            var instructorUser = new ApplicationUser { Id = instructorUserId, UserName = "instructor" };

            mockUserManager.Setup(m => m.FindByIdAsync(instructorUserId.ToString()))
                .ReturnsAsync(instructorUser);
            mockUserManager.Setup(m => m.IsInRoleAsync(instructorUser, "Admin"))
                .ReturnsAsync(false);

            var courseId = Guid.NewGuid();
            var course = new Course
            {
                Id = courseId,
                Title = "Orphaned Course",
                InstructorProfileId = Guid.NewGuid(),
                Level = CourseLevel.Beginner
            };
            context.Courses.Add(course);
            await context.SaveChangesAsync();

            var handler = new ArchiveCourseHandler(context, mockUserManager.Object);

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                await handler.Handle(new ArchiveCourseCommand(courseId, instructorUserId), CancellationToken.None)
            );

            // Assert
            Assert.NotNull(exception);
            Assert.IsNotType<NullReferenceException>(exception);
            Assert.Equal("Course not found", exception.Message);
        }

        [Fact]
        public async Task Instructor_Calls_Unpublish_On_Orphaned_Course_Does_Not_Throw_NullRef()
        {
            // Arrange
            using var context = GetDbContext();
            var mockUserManager = GetMockUserManager();

            var instructorUserId = Guid.NewGuid();
            var instructorUser = new ApplicationUser { Id = instructorUserId, UserName = "instructor" };

            mockUserManager.Setup(m => m.FindByIdAsync(instructorUserId.ToString()))
                .ReturnsAsync(instructorUser);
            mockUserManager.Setup(m => m.IsInRoleAsync(instructorUser, "Admin"))
                .ReturnsAsync(false);

            var courseId = Guid.NewGuid();
            var course = new Course
            {
                Id = courseId,
                Title = "Orphaned Course",
                InstructorProfileId = Guid.NewGuid(),
                Level = CourseLevel.Beginner
            };
            context.Courses.Add(course);
            await context.SaveChangesAsync();

            var handler = new UnpublishCourseHandler(context, mockUserManager.Object);

            // Act
            var exception = await Record.ExceptionAsync(async () =>
                await handler.Handle(new UnpublishCourseCommand(courseId, instructorUserId), CancellationToken.None)
            );

            // Assert
            Assert.NotNull(exception);
            Assert.IsNotType<NullReferenceException>(exception);
            Assert.Equal("Course not found", exception.Message);
        }

        [Fact]
        public async Task CreateCourse_ShouldDefault_RequiresApproval_False()
        {
            using var context = GetDbContext();
            var mockCurrentUser = new Mock<ICurrentUserService>();
            var instructorUserId = Guid.NewGuid();
            mockCurrentUser.Setup(x => x.UserId).Returns(instructorUserId);

            var profile = new InstructorProfile 
            { 
                Id = Guid.NewGuid(), 
                UserId = instructorUserId,
                Bio = "test"
            };
            context.InstructorProfiles.Add(profile);
            await context.SaveChangesAsync();

            var handler = new CreateCourseHandler(context, mockCurrentUser.Object);
            var command = new CreateCourseCommand(
                profile.Id,
                "Test Course",
                "Test Description",
                100,
                "Arabic",
                CourseLevel.Beginner,
                "Arabic",
                null
            );

            var courseId = await handler.Handle(command, CancellationToken.None);

            var course = await context.Courses.FindAsync(courseId);
            Assert.NotNull(course);
            Assert.False(course.RequiresApproval);
            Assert.False(course.IsPublished);
        }

        [Fact]
        public async Task CreateCourse_ThenAddModule_ShouldSucceed()
        {
            using var context = GetDbContext();
            var mockCurrentUser = new Mock<ICurrentUserService>();
            var instructorUserId = Guid.NewGuid();
            mockCurrentUser.Setup(x => x.UserId).Returns(instructorUserId);

            var profile = new InstructorProfile 
            { 
                Id = Guid.NewGuid(), 
                UserId = instructorUserId,
                Bio = "test"
            };
            context.InstructorProfiles.Add(profile);
            await context.SaveChangesAsync();

            // Create course
            var createHandler = new CreateCourseHandler(context, mockCurrentUser.Object);
            var courseId = await createHandler.Handle(new CreateCourseCommand(
                profile.Id,
                "Test Course",
                "Test",
                100,
                "Arabic",
                CourseLevel.Beginner,
                "Arabic",
                null
            ), CancellationToken.None);

            // Now try to add a module immediately using actual codebase CreateModuleHandler
            var createModuleHandler = new CreateModuleHandler(context);
            var moduleId = await createModuleHandler.Handle(new CreateModuleCommand
            {
                CourseId = courseId,
                Title = "Module 1",
                OrderIndex = 1
            }, CancellationToken.None);

            var module = await context.Modules.FindAsync(moduleId);
            Assert.NotNull(module);
            Assert.Equal(courseId, module.CourseId);
        }

        [Fact]
        public async Task Test_C_CourseCreated_StatusShouldBeDraft()
        {
            using var context = GetDbContext();
            var mockCurrentUser = new Mock<ICurrentUserService>();
            var instructorUserId = Guid.NewGuid();
            mockCurrentUser.Setup(x => x.UserId).Returns(instructorUserId);

            var profile = new InstructorProfile 
            { 
                Id = Guid.NewGuid(), 
                UserId = instructorUserId,
                Bio = "test"
            };
            context.InstructorProfiles.Add(profile);
            await context.SaveChangesAsync();

            var handler = new CreateCourseHandler(context, mockCurrentUser.Object);
            var command = new CreateCourseCommand(
                profile.Id,
                "Test Course",
                "Test Description",
                100,
                "Arabic",
                CourseLevel.Beginner,
                "Arabic",
                null
            );

            var courseId = await handler.Handle(command, CancellationToken.None);

            var course = await context.Courses.FindAsync(courseId);
            Assert.NotNull(course);
            Assert.Equal(CourseStatus.Draft, course.Status);
        }

        [Fact]
        public async Task Test_D_AddModuleToDraftCourse_ShouldSucceed()
        {
            using var context = GetDbContext();
            var mockCurrentUser = new Mock<ICurrentUserService>();
            var instructorUserId = Guid.NewGuid();
            mockCurrentUser.Setup(x => x.UserId).Returns(instructorUserId);

            var profile = new InstructorProfile 
            { 
                Id = Guid.NewGuid(), 
                UserId = instructorUserId,
                Bio = "test"
            };
            context.InstructorProfiles.Add(profile);
            await context.SaveChangesAsync();

            var createHandler = new CreateCourseHandler(context, mockCurrentUser.Object);
            var courseId = await createHandler.Handle(new CreateCourseCommand(
                profile.Id,
                "Test Course",
                "Test",
                100,
                "Arabic",
                CourseLevel.Beginner,
                "Arabic",
                null
            ), CancellationToken.None);

            var createModuleHandler = new CreateModuleHandler(context);
            var moduleId = await createModuleHandler.Handle(new CreateModuleCommand
            {
                CourseId = courseId,
                Title = "Module 1",
                OrderIndex = 1
            }, CancellationToken.None);

            var module = await context.Modules.FindAsync(moduleId);
            Assert.NotNull(module);
            Assert.Equal(courseId, module.CourseId);
        }

        [Fact]
        public async Task Test_E_SetCourseStatusUnderReview_AddModule_Throws()
        {
            using var context = GetDbContext();
            var mockCurrentUser = new Mock<ICurrentUserService>();
            var instructorUserId = Guid.NewGuid();
            mockCurrentUser.Setup(x => x.UserId).Returns(instructorUserId);

            var profile = new InstructorProfile 
            { 
                Id = Guid.NewGuid(), 
                UserId = instructorUserId,
                Bio = "test"
            };
            context.InstructorProfiles.Add(profile);
            await context.SaveChangesAsync();

            var createHandler = new CreateCourseHandler(context, mockCurrentUser.Object);
            var courseId = await createHandler.Handle(new CreateCourseCommand(
                profile.Id,
                "Test Course",
                "Test",
                100,
                "Arabic",
                CourseLevel.Beginner,
                "Arabic",
                null
            ), CancellationToken.None);

            var course = await context.Courses.FindAsync(courseId);
            Assert.NotNull(course);
            course.Status = CourseStatus.UnderReview;
            await context.SaveChangesAsync();

            var createModuleHandler = new CreateModuleHandler(context);
            var exception = await Record.ExceptionAsync(async () =>
                await createModuleHandler.Handle(new CreateModuleCommand
                {
                    CourseId = courseId,
                    Title = "Module 1",
                    OrderIndex = 1
                }, CancellationToken.None)
            );

            Assert.NotNull(exception);
            Assert.IsType<InvalidOperationException>(exception);
            Assert.Equal("Cannot add modules while the course is under review.", exception.Message);
        }

        [Fact]
        public async Task Test_F_SetCourseStatusRejected_AddModule_Succeeds()
        {
            using var context = GetDbContext();
            var mockCurrentUser = new Mock<ICurrentUserService>();
            var instructorUserId = Guid.NewGuid();
            mockCurrentUser.Setup(x => x.UserId).Returns(instructorUserId);

            var profile = new InstructorProfile 
            { 
                Id = Guid.NewGuid(), 
                UserId = instructorUserId,
                Bio = "test"
            };
            context.InstructorProfiles.Add(profile);
            await context.SaveChangesAsync();

            var createHandler = new CreateCourseHandler(context, mockCurrentUser.Object);
            var courseId = await createHandler.Handle(new CreateCourseCommand(
                profile.Id,
                "Test Course",
                "Test",
                100,
                "Arabic",
                CourseLevel.Beginner,
                "Arabic",
                null
            ), CancellationToken.None);

            var course = await context.Courses.FindAsync(courseId);
            Assert.NotNull(course);
            course.Status = CourseStatus.Rejected;
            await context.SaveChangesAsync();

            var createModuleHandler = new CreateModuleHandler(context);
            var moduleId = await createModuleHandler.Handle(new CreateModuleCommand
            {
                CourseId = courseId,
                Title = "Module 1",
                OrderIndex = 1
            }, CancellationToken.None);

            var module = await context.Modules.FindAsync(moduleId);
            Assert.NotNull(module);
            Assert.Equal(courseId, module.CourseId);
        }

        [Fact]
        public async Task Test_PublishedCourse_AddModule_Succeeds()
        {
            using var context = GetDbContext();
            var mockCurrentUser = new Mock<ICurrentUserService>();
            var instructorUserId = Guid.NewGuid();
            mockCurrentUser.Setup(x => x.UserId).Returns(instructorUserId);

            var profile = new InstructorProfile 
            { 
                Id = Guid.NewGuid(), 
                UserId = instructorUserId,
                Bio = "test"
            };
            context.InstructorProfiles.Add(profile);
            await context.SaveChangesAsync();

            var createHandler = new CreateCourseHandler(context, mockCurrentUser.Object);
            var courseId = await createHandler.Handle(new CreateCourseCommand(
                profile.Id,
                "Test Course",
                "Test",
                100,
                "Arabic",
                CourseLevel.Beginner,
                "Arabic",
                null
            ), CancellationToken.None);

            var course = await context.Courses.FindAsync(courseId);
            Assert.NotNull(course);
            course.Status = CourseStatus.Published;
            await context.SaveChangesAsync();

            var createModuleHandler = new CreateModuleHandler(context);
            var moduleId = await createModuleHandler.Handle(new CreateModuleCommand
            {
                CourseId = courseId,
                Title = "Module 1",
                OrderIndex = 1
            }, CancellationToken.None);

            var module = await context.Modules.FindAsync(moduleId);
            Assert.NotNull(module);
            Assert.Equal(courseId, module.CourseId);
        }
    }
}
