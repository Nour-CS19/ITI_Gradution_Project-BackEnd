using Femora.Domain.Entities.LMS;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Femora.Application.Common.Interfaces.Repositories.LMS
{
    public interface ICourseRepository
    {
        Task<IEnumerable<Course>> GetByInstructorAsync(Guid instructorProfileId);
    }
}