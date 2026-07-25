using Femora.Application.Common.Interfaces.Repositories;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Femora.Infrastructure.Repositories;

public class LessonPdfRepository : ILessonPdfRepository
{
    static LessonPdfRepository()
    {
        // Community (free) license - fine for a student graduation project.
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public byte[] GenerateKeyQuestionsPdf(string lessonTitle, string courseTitle, IReadOnlyList<PdfQuestionItem> items)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(35);
                // Arabic content reads right-to-left; QuestPDF mirrors the whole
                // layout (margins, alignment, list order) when this is set.
                page.ContentFromRightToLeft();
                page.DefaultTextStyle(x => x.FontFamily("Arial").FontSize(12));

                page.Header().Column(col =>
                {
                    col.Item().Text("Femora").FontSize(11).FontColor(Colors.Grey.Medium).Bold();
                    col.Item().PaddingTop(2).Text(lessonTitle).FontSize(20).Bold();
                    if (!string.IsNullOrWhiteSpace(courseTitle))
                        col.Item().PaddingTop(2).Text(courseTitle).FontSize(12).FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingTop(8).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().PaddingTop(15).Column(col =>
                {
                    col.Spacing(14);

                    if (items.Count == 0)
                    {
                        col.Item().Text("لا توجد أسئلة كافية لإنشاء هذا الملف بعد.")
                            .FontSize(13).FontColor(Colors.Grey.Darken2);
                        return;
                    }

                    for (var i = 0; i < items.Count; i++)
                    {
                        var item = items[i];
                        col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(12).Column(qCol =>
                        {
                            qCol.Spacing(6);
                            qCol.Item().Text($"{i + 1}. {item.Question}").Bold().FontSize(13);
                            qCol.Item().Text(item.Answer).FontSize(12).FontColor(Colors.Grey.Darken3);
                        });
                    }
                });

                page.Footer().AlignCenter().Text(text =>
                {
                    text.Span("تم إنشاؤه تلقائيًا بواسطة مساعد Femora الذكي - ");
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }
}
