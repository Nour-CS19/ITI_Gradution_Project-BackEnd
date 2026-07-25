using Femora.Application.Common.Interfaces.Repositories;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Femora.Application.Features.Admin.Commands.ResetAndReindexVideoLessons;

public class ResetAndReindexVideoLessonsCommandHandler(ILessonIndexingRepository lessonIndexingRepository)
    : IRequestHandler<ResetAndReindexVideoLessonsCommand, ResetAndReindexVideoLessonsResponse>
{
    public async Task<ResetAndReindexVideoLessonsResponse> Handle(
        ResetAndReindexVideoLessonsCommand request, CancellationToken cancellationToken)
    {
        var (succeeded, failed) = await lessonIndexingRepository.ResetAndReindexAllVideoLessonsAsync(cancellationToken);

        return new ResetAndReindexVideoLessonsResponse
        {
            Succeeded = succeeded,
            Failed = failed
        };
    }
}
