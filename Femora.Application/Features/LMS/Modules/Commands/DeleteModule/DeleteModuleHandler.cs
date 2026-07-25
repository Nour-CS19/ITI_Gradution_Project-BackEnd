using Femora.Application.Common.Interfaces;
using Femora.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Femora.Application.Features.LMS.Modules.Commands.DeleteModule
{
    public class DeleteModuleHandler(IAppDbContext context) : IRequestHandler<DeleteModuleCommand, bool>
    {
        public async Task<bool> Handle(DeleteModuleCommand request, CancellationToken cancellationToken)
        {
            var module = await context.Modules.FindAsync(new object[] { request.Id }, cancellationToken);

            if (module == null)
                return false;

            var courseStatus = await context.Courses
                .Where(c => c.Id == module.CourseId)
                .Select(c => c.Status)
                .FirstOrDefaultAsync(cancellationToken);

            if (courseStatus == CourseStatus.UnderReview)
                throw new InvalidOperationException("Cannot delete modules while the course is under review.");

            context.Modules.Remove(module);
            await context.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
