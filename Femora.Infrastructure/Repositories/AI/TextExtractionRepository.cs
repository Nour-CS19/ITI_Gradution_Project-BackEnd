using Azure;
using Azure.AI.OpenAI;
using Femora.Application.Common.Interfaces.Repositories;
using Femora.Infrastructure.Options;
using Microsoft.Extensions.Options;
using OpenAI.Audio;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UglyToad.PdfPig;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace Femora.Infrastructure.Repositories;

public class TextExtractionRepository : ITextExtractionRepository
{
    private readonly AudioClient _audioClient;

    public TextExtractionRepository(IOptions<AzureOpenAIOptions> options)
    {
        var settings = options.Value;
        var azureClient = new AzureOpenAIClient(new Uri(settings.Endpoint), new AzureKeyCredential(settings.ApiKey));
        _audioClient = azureClient.GetAudioClient(settings.WhisperDeploymentName);
    }

    public Task<string> ExtractTextFromPdfAsync(Stream pdfStream, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var sb = new StringBuilder();
        using var document = PdfDocument.Open(pdfStream);
        foreach (var page in document.GetPages())
        {
            cancellationToken.ThrowIfCancellationRequested();
            sb.AppendLine(page.Text);
            sb.AppendLine();
        }
        return Task.FromResult(sb.ToString().Trim());
    }

    public Task<IReadOnlyList<string>> ExtractPagesFromPdfAsync(Stream pdfStream, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var pages = new List<string>();
        using var document = PdfDocument.Open(pdfStream);
        foreach (var page in document.GetPages())
        {
            cancellationToken.ThrowIfCancellationRequested();
            pages.Add(page.Text);
        }
        return Task.FromResult<IReadOnlyList<string>>(pages);
    }

    public Task<string> ExtractTextFromDocxAsync(Stream docxStream, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var sb = new StringBuilder();

        using var wordDocument = WordprocessingDocument.Open(docxStream, isEditable: false);
        var body = wordDocument.MainDocumentPart?.Document?.Body;
        if (body is null)
            return Task.FromResult(string.Empty);

        foreach (var paragraph in body.Descendants<Paragraph>())
        {
            cancellationToken.ThrowIfCancellationRequested();
            sb.AppendLine(paragraph.InnerText);
        }

        return Task.FromResult(sb.ToString().Trim());
    }

    // Whisper accepts mp4/mov/webm/mp3/wav/m4a directly (it reads the audio
    // track), so lesson videos don't need to be demuxed first - the raw
    // video stream is sent as-is, just like PdfPig/OpenXml read PDF/DOCX
    // bytes directly above.
    // Azure OpenAI's Whisper deployment hard-rejects any file over 25MB, and this
    // project has no audio-extraction/compression step (no FFMpeg/NAudio in the
    // pipeline) - the raw video bytes go straight to Whisper. Without this check,
    // an oversized video would fail deep inside the Azure SDK with an opaque error;
    // this turns it into a clear, actionable message shown on the LessonResource.
    private const long WhisperMaxFileSizeBytes = 25 * 1024 * 1024;

    public async Task<string> ExtractTextFromVideoAsync(Stream videoStream, string fileName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (videoStream.CanSeek && videoStream.Length > WhisperMaxFileSizeBytes)
        {
            var sizeMb = videoStream.Length / (1024.0 * 1024.0);
            throw new InvalidOperationException(
                $"الفيديو حجمه {sizeMb:0.#} ميجابايت ويتجاوز الحد الأقصى لخدمة تفريغ الصوت (25 ميجابايت). " +
                "قصّري الفيديو أو صغّري جودته (أو استخرجي الصوت فقط بصيغة mp3/m4a مضغوطة) وأعيدي رفعه.");
        }

        var options = new AudioTranscriptionOptions
        {
            ResponseFormat = AudioTranscriptionFormat.Text,
        };

        var result = await _audioClient.TranscribeAudioAsync(videoStream, fileName, options, cancellationToken);
        return result.Value.Text?.Trim() ?? string.Empty;
    }
}
