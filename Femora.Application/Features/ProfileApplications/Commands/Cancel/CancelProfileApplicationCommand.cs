using System;
using System.Threading;
using System.Threading.Tasks;
using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.ProfileApplications.Commands.Cancel;

public class CancelProfileApplicationCommand : IRequest
{
    public Guid UserId { get; set; }
}

public class CancelProfileApplicationCommandHandler : IRequestHandler<CancelProfileApplicationCommand>
{
    private readonly IAppDbContext _context;

    public CancelProfileApplicationCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(CancelProfileApplicationCommand request, CancellationToken cancellationToken)
    {
        var application = await _context.ProfileApplicationRequests
            .FirstOrDefaultAsync(r => r.UserId == request.UserId && r.Status == ApplicationRequestStatus.Pending, cancellationToken);

        if (application == null)
            throw new NotFoundException("ProfileApplicationRequest", "Pending request for user not found");

        application.Status = ApplicationRequestStatus.Cancelled;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
