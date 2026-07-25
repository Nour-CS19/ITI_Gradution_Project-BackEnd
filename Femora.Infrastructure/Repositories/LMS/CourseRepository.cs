using Femora.Application.Common.Interfaces;
using Femora.Application.Common.Interfaces.Repositories.LMS;
using Femora.Domain.Entities.LMS;
using Femora.Infrastructure.Data;
using Femora.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Femora.Infrastructure.Repositoies.LMS
{
    public class CourseRepository(IAppDbContext context) : ICourseRepository
    {
        public async Task<IEnumerable<Course>> GetByInstructorAsync(Guid instructorProfileId)
        {
            return await context.Courses
                .AsNoTracking()
                .Where(c => c.InstructorProfileId == instructorProfileId)
                .ToListAsync();
        }
    }
}