using FluentValidation;

namespace Femora.Application.Features.LMS.Commands.UploadLessonResource;

public class UploadLessonResourceCommandValidator : AbstractValidator<UploadLessonResourceCommand>
{
    private static readonly string[] AllowedContentTypes =
    [
        "application/pdf",
        "text/plain",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        // Lesson videos: transcribed via Whisper and indexed for RAG search
        // exactly like PDF/DOCX text (see LessonIndexingRepository).
        "video/mp4",
        "video/webm",
        "video/quicktime",
        "video/x-msvideo",      // .avi files
        "video/mpeg",           // .mpeg/.mpg files
        "video/x-matroska"      // .mkv files
    ];

    public UploadLessonResourceCommandValidator()
    {
        RuleFor(x => x.LessonId)
            .NotEmpty().WithMessage("LessonId is required.");

        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("FileName is required.")
            .MaximumLength(255).WithMessage("FileName must not exceed 255 characters.");

        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("ContentType is required.")
            .Must(ct => AllowedContentTypes.Contains(ct))
            .WithMessage("Only PDF, TXT, Word documents, and video files (MP4, WebM, MOV) are supported.");

        RuleFor(x => x.FileStream)
            .NotNull().WithMessage("File stream is required.");
    }
}