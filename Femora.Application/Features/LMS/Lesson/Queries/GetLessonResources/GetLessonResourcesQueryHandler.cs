using Femora.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Femora.Application.Features.LMS.Lesson.Queries.GetLessonResources;

/// <summary>
/// Lets instructors see WHY a lesson's video/PDF isn't indexed instead of guessing -
/// surfaces the exact Status + ErrorMessage recorded by LessonIndexingRepository
/// (e.g. seeding hit an Azure rate limit, a bad Whisper file format, etc.) that would
/// otherwise only be visible in server console logs.
/// </summary>
public class GetLessonResourcesQueryHandler(IAppDbContext db)
    : IRequestHandler<GetLessonResourcesQuery, System.Collections.Generic.List<LessonResourceStatusDto>>
{
    public async Task<System.Collections.Generic.List<LessonResourceStatusDto>> Handle(
        GetLessonResourcesQuery request, CancellationToken cancellationToken)
    {
        return await db.LessonResources
            .AsNoTracking()
            .Where(r => r.LessonId == request.LessonId)
            .OrderByDescending(r => r.UploadedAt)
            .Select(r => new LessonResourceStatusDto
            {
                Id = r.Id,
                FileName = r.FileName,
                Status = r.Status.ToString(),
                ChunkCount = r.ChunkCount,
                ErrorMessage = r.ErrorMessage,
                UploadedAt = r.UploadedAt,
                IndexedAt = r.IndexedAt
            })
            .ToListAsync(cancellationToken);
    }
}
