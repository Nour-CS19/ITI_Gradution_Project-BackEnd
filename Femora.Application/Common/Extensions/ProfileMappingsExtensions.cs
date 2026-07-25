using Femora.Application.Features.Identity.Common.DTOs;
using Femora.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Application.Common.Extensions;
public static class ProfileMappingsExtensions
{
    public static AvailableProfileDto ToDto(this ProfileType profile) =>
    profile switch
    {
        ProfileType.Trainee => new AvailableProfileDto
        {
            Id = 1,
            Name = "Trainee",
            DisplayName = "Learning Workspace",
            Description = "Courses, exams and assignments",
            Icon = "graduation-cap"
        },

        ProfileType.Instructor => new AvailableProfileDto
        {
            Id = 2,
            Name = "Instructor",
            DisplayName = "Instructor Studio",
            Description = "Manage courses and students",
            Icon = "chalkboard"
        },

        ProfileType.Seller => new AvailableProfileDto
        {
            Id = 3,
            Name = "Seller",
            DisplayName = "Seller Hub",
            Description = "Manage products and orders",
            Icon = "store"
        },

        _ => throw new ArgumentOutOfRangeException(nameof(profile))
    };
}
