using Femora.Application.Common.DTOs;
using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Femora.Application.Features.Ai.Queries.SuggestedQuestions;

public class GetLessonSuggestedQuestionsQueryHandler(IAppDbContext db)
    : IRequestHandler<GetLessonSuggestedQuestionsQuery, List<SuggestedQuestionDto>>
{
    // Templates only - never invent lesson facts here, just generic prompts that
    // point the model at the lesson's own indexed content via {title}. The actual
    // answer always comes from ChatWithLessonCommandHandler's RAG retrieval.
    private static readonly string[] Templates =
    {
        "اشرحيلي أهم نقطة في \"{title}\" بشكل مبسط",
        "ايه أهم حاجة لازم أركز فيها في \"{title}\"؟",
        "فيه أمثلة عملية على \"{title}\"؟",
        "لخصيلي \"{title}\" في نقطتين بس",
        "ايه العلاقة بين \"{title}\" وباقي الدرس؟",
        "لو نسيت حاجة في \"{title}\"، ابدئي بإيه؟",
    };

    public async Task<List<SuggestedQuestionDto>> Handle(
        GetLessonSuggestedQuestionsQuery request,
        CancellationToken cancellationToken)
    {
        var lesson = await db.Lessons
            .Where(l => l.Id == request.LessonId)
            .Select(l => new { l.Title })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException("Lesson", request.LessonId.ToString());

        var title = string.IsNullOrWhiteSpace(lesson.Title) ? "الدرس ده" : lesson.Title;

        // Rotate every ~3 hours, varied per-lesson so different lessons don't always
        // show the exact same subset in the exact same order.
        var timeBucket = DateTime.UtcNow.Ticks / TimeSpan.FromHours(3).Ticks;
        var seed = unchecked((int)timeBucket) ^ request.LessonId.GetHashCode();
        var rng = new Random(seed);

        var count = request.Count <= 0 ? 4 : request.Count;

        return Templates
            .OrderBy(_ => rng.Next())
            .Take(Math.Min(count, Templates.Length))
            .Select(t => new SuggestedQuestionDto { Question = t.Replace("{title}", title) })
            .ToList();
    }
}
