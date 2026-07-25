using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.Admin.Commands.SetUserActive
{
    public sealed class SetUserActiveCommandHandler(IAppDbContext db, ICurrentUserService currentUser)
        : IRequestHandler<SetUserActiveCommand>
    {
        public async Task Handle(SetUserActiveCommand request, CancellationToken cancellationToken)
        {
            if (!request.IsActive && request.UserId == currentUser.UserId)
            {
                throw new ValidationException("You cannot deactivate your own account.");
            }

            var updated = await db.ApplicationUsers
                .Where(u => u.Id == request.UserId)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(u => u.IsActive, request.IsActive),
                    cancellationToken);

            if (updated == 0)
            {
                throw new NotFoundException("User", request.UserId.ToString());
            }
        }
    }
}
