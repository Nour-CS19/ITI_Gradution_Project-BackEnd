using Femora.Application.Common.Interfaces;
using Femora.Application.Common.Interfaces.Repositories.LMS;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Femora.Infrastructure.Repositoies.LMS;

public class QuizRepository(IAppDbContext context) : IQuizRepository
{
    public async Task<bool> HasPassedAsync(Guid enrollmentId, Guid moduleId, CancellationToken ct = default)
    {
        var quiz = await context.Quizzes.FirstOrDefaultAsync(q => q.ModuleId == moduleId, ct);

        if (quiz == null) return false;

        var latestAttempt = await context.QuizAttempts.Where(a => a.QuizId == quiz.Id && a.EnrollmentId == enrollmentId)
            .OrderByDescending(a => a.AttemptedAt)
            .FirstOrDefaultAsync(ct);

        return latestAttempt?.IsPassed ?? false;
    }
}
