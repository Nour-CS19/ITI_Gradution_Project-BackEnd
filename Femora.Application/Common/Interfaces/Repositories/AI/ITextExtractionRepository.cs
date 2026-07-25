using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Femora.Application.Common.Interfaces.Repositories;

public interface ITextExtractionRepository
{
    Task<string> ExtractTextFromPdfAsync(Stream pdfStream, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<string>> ExtractPagesFromPdfAsync(Stream pdfStream, CancellationToken cancellationToken = default);
    Task<string> ExtractTextFromDocxAsync(Stream docxStream, CancellationToken cancellationToken = default);

    /// <summary>
    /// Transcribes a lesson video's audio track (Whisper) so the spoken
    /// content can be indexed for RAG search exactly like PDF/DOCX text.
    /// </summary>
    Task<string> ExtractTextFromVideoAsync(Stream videoStream, string fileName, CancellationToken cancellationToken = default);
}
