namespace Femora.Application.Common.Interfaces.Repositories;

public record PdfQuestionItem(string Question, string Answer);

public interface ILessonPdfRepository
{
    /// <summary>
    /// Renders a simple RTL-friendly "key questions" study sheet as PDF bytes.
    /// </summary>
    byte[] GenerateKeyQuestionsPdf(string lessonTitle, string courseTitle, IReadOnlyList<PdfQuestionItem> items);
}
