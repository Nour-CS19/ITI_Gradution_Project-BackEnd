using Femora.Application.Common.DTOs;
using Femora.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Femora.Application.Features.Ai.Queries.SuggestedQuestions;

public class GetSuggestedQuestionsQueryHandler(IAppDbContext db)
    : IRequestHandler<GetSuggestedQuestionsQuery, List<SuggestedQuestionDto>>
{
    // Curated, real questions about how Femora actually works - kept generic on
    // purpose (no invented course/product names) so they're always accurate,
    // regardless of what's currently in the catalog.
    private static readonly string[] GeneralPool =
    {
        "إزاي أختار الكورس المناسب لمستوايا؟",
        "إزاي أقدر أفتح الوحدة (module) الجاية في الكورس؟",
        "هل ممكن أرجع أراجع اختبار الوحدة تاني لو مش عدّيته؟",
        "إزاي أضيف منتج جديد في المتجر؟",
        "إزاي أظبط اهتماماتي عشان الاقتراحات تبقى أدق؟",
        "فرق ايه بين التدريب والمتجر في Femora؟",
        "إزاي أتواصل مع المساعد الذكي لو واجهتني مشكلة في درس معين؟",
        "إزاي أعرف تقدمي في الكورس اللي مسجلة فيه؟",
        "هل فيه طريقة ألخص بيها أي درس بسرعة؟",
        "إزاي أسعّر منتجاتي بشكل عادل في السوق المصري؟",
        "ليه مش قادرة أفتح درس في وحدة معينة؟",
        "إزاي أعرف لو نجحت في اختبار الوحدة ولا لأ؟",
    };

    // Extra questions shown only for users who also have a seller profile.
    private static readonly string[] SellerPool =
    {
        "إزاي أحسّن وصف منتجي عشان يبقى أوضح للمشترين؟",
        "إزاي أعرف السعر المناسب لمنتج جديد قبل ما أنشره؟",
    };

    public async Task<List<SuggestedQuestionDto>> Handle(
        GetSuggestedQuestionsQuery request,
        CancellationToken cancellationToken)
    {
        var isSeller = await db.SellerProfiles.AnyAsync(s => s.UserId == request.UserId, cancellationToken);

        var pool = isSeller
            ? GeneralPool.Concat(SellerPool).ToList()
            : GeneralPool.ToList();

        // Rotate the selection every ~3 hours, and vary a little per-user so not
        // every user sees the exact same five questions at the exact same time.
        var timeBucket = DateTime.UtcNow.Ticks / TimeSpan.FromHours(3).Ticks;
        var seed = unchecked((int)timeBucket) ^ request.UserId.GetHashCode();
        var rng = new Random(seed);

        var count = request.Count <= 0 ? 5 : request.Count;

        return pool
            .OrderBy(_ => rng.Next())
            .Take(Math.Min(count, pool.Count))
            .Select(q => new SuggestedQuestionDto { Question = q })
            .ToList();
    }
}
