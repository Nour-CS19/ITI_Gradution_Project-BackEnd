using AutoMapper;
using Femora.Application.Features.Identity.Commands.Login;
using Femora.Application.Features.Identity.Commands.Register;
using Femora.Application.Features.Identity.Common.Requests;
using Femora.Application.Features.LMS.Courses.DTOs;
using Femora.Application.Features.LMS.Lesson.DTOs;
using Femora.Application.Features.LMS.Modules.DTOs;
using Femora.Application.Features.LMS.Quizzes.DTOs;
using Femora.Domain.Entities.LMS;
using Femora.Domain.Entities.LMS.Quizzes;

namespace Femora.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // =========================
        // 🔐 Identity Mappings
        // =========================
        CreateMap<RegisterCommand, RegisterRequest>();
        CreateMap<SigninCommand, SigninRequest>();

        // =========================
        // 📚 LMS Course Mappings
        // =========================
        CreateMap<Course, CourseSummaryDto>();

        CreateMap<Course, CourseDetailsDto>();
        // =========================
        // 📚 LMS Module Mappings
        // =========================
        CreateMap<Module, ModuleDto>();

        CreateMap<Module, ModuleDetailsDto>();

        CreateMap<Lesson, LessonDto>();

        CreateMap<Quiz, QuizDto>();
    }
}