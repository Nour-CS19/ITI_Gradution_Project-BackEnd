using Femora.Application.Common.Interfaces;
using Femora.Application.Features.LMS.Lesson.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.LMS.Lesson.Queries;

public class GetLessonsByModuleHandler
    : IRequestHandler<GetLessonsByModuleQuery, List<LessonDto>>
{
    private readonly IAppDbContext _context;

    public GetLessonsByModuleHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<LessonDto>> Handle(GetLessonsByModuleQuery request, CancellationToken cancellationToken)
    {
        return await _context.Lessons
            .Where(x => x.ModuleId == request.ModuleId)
            .OrderBy(x => x.OrderIndex)
            .Select(x => new LessonDto
            {
                Id = x.Id,
                ModuleId = x.ModuleId,
                Title = x.Title,
                Type = x.Type,
                ArticleContent = x.ArticleContent,
                ContentUrl = x.ContentUrl,
                ContentType = x.Type.ToString(),
                ContentText = x.ArticleContent,
                ContentMimeType = x.LessonResources.OrderBy(r => r.UploadedAt).Select(r => r.ContentType).FirstOrDefault(),
                DurationSeconds = x.DurationSeconds ?? 0,
                OrderIndex = x.OrderIndex,
                IsPreview = x.IsPreview
            })
            .ToListAsync(cancellationToken);
    }
}
