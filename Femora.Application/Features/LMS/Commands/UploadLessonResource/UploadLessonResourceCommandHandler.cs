using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Application.Common.Interfaces.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.LMS.Commands.UploadLessonResource;

public class UploadLessonResourceCommandHandler(
    IAppDbContext db,
    ILessonIndexingRepository lessonIndexingRepository)
    : IRequestHandler<UploadLessonResourceCommand, UploadLessonResourceResponse>
{
    public async Task<UploadLessonResourceResponse> Handle(
        UploadLessonResourceCommand request,
        CancellationToken cancellationToken)
    {
        var lesson = await db.Lessons
            .FirstOrDefaultAsync(l => l.Id == request.LessonId, cancellationToken)
            ?? throw new NotFoundException("Lesson", request.LessonId.ToString());

        var lessonResourceId = await lessonIndexingRepository.UploadAndIndexLessonResourceAsync(
            lessonId: request.LessonId,
            fileStream: request.FileStream,
            fileName: request.FileName,
            contentType: request.ContentType,
            cancellationToken: cancellationToken
        );

        return new UploadLessonResourceResponse
        {
            LessonResourceId = lessonResourceId,
            FileName = request.FileName,
            Message = "File uploaded and indexing started successfully."
        };
    }
}
