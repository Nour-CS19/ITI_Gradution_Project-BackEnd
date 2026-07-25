using System;
using System.Threading;
using System.Threading.Tasks;
using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.ProfileApplications.Commands.Reject;

public class RejectProfileApplicationCommand : IRequest
{
    public Guid Id { get; set; }
    public Guid AdminUserId { get; set; }
    public string RejectionReason { get; set; } = string.Empty;
}

public class RejectProfileApplicationCommandValidator : AbstractValidator<RejectProfileApplicationCommand>
{
    public RejectProfileApplicationCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.AdminUserId).NotEmpty();
        RuleFor(x => x.RejectionReason).NotEmpty().MaximumLength(500);
    }
}

public class RejectProfileApplicationCommandHandler : IRequestHandler<RejectProfileApplicationCommand>
{
    private readonly IAppDbContext _context;

    public RejectProfileApplicationCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(RejectProfileApplicationCommand request, CancellationToken cancellationToken)
    {
        var application = await _context.ProfileApplicationRequests
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (application == null)
            throw new NotFoundException("ProfileApplicationRequest", request.Id.ToString());

        if (application.Status != ApplicationRequestStatus.Pending)
            throw new InvalidOperationException("Only Pending applications can be rejected.");

        application.Status = ApplicationRequestStatus.Rejected;
        application.ReviewedAt = DateTime.UtcNow;
        application.ReviewedByAdminId = request.AdminUserId;
        application.RejectionReason = request.RejectionReason;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
