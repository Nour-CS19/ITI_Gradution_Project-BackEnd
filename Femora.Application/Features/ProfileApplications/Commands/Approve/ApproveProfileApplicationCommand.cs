using System;
using System.Threading;
using System.Threading.Tasks;
using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Domain.Entities.Identity;
using Femora.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.ProfileApplications.Commands.Approve;

public class ApproveProfileApplicationCommand : IRequest
{
    public Guid Id { get; set; }
    public Guid AdminUserId { get; set; }
}

public class ApproveProfileApplicationCommandHandler : IRequestHandler<ApproveProfileApplicationCommand>
{
    private readonly IAppDbContext _context;

    public ApproveProfileApplicationCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(ApproveProfileApplicationCommand request, CancellationToken cancellationToken)
    {
        var application = await _context.ProfileApplicationRequests
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken);

        if (application == null)
            throw new NotFoundException("ProfileApplicationRequest", request.Id.ToString());

        if (application.Status != ApplicationRequestStatus.Pending)
            throw new InvalidOperationException("Only Pending applications can be approved.");

        using var transaction = await _context.BeginTransactionAsync(cancellationToken);
        try
        {
            // 1. Update application status
            application.Status = ApplicationRequestStatus.Approved;
            application.ReviewedAt = DateTime.UtcNow;
            application.ReviewedByAdminId = request.AdminUserId;

            // 2. Create the corresponding profile (idempotent check)
            if (application.RequestedRole == RequestedRole.Instructor)
            {
                var profileExists = await _context.InstructorProfiles
                    .AnyAsync(ip => ip.UserId == application.UserId, cancellationToken);

                if (!profileExists)
                {
                    var instructorProfile = new InstructorProfile
                    {
                        Id = Guid.NewGuid(),
                        UserId = application.UserId,
                        Specialization = "General",
                        Bio = application.Bio ?? string.Empty,
                        Status = VerificationStatus.Approved,
                        VerifiedByAdminId = request.AdminUserId,
                        VerifiedAt = DateTime.UtcNow
                    };
                    _context.InstructorProfiles.Add(instructorProfile);
                }
            }
            else if (application.RequestedRole == RequestedRole.Seller)
            {
                var profileExists = await _context.SellerProfiles
                    .AnyAsync(sp => sp.UserId == application.UserId, cancellationToken);

                if (!profileExists)
                {
                    var sellerProfile = new SellerProfile
                    {
                        Id = Guid.NewGuid(),
                        UserId = application.UserId,
                        StoreName = application.StoreName ?? "Store",
                        StoreDescription = application.StoreDescription ?? "Store description",
                        Status = VerificationStatus.Approved,
                        VerifiedByAdminId = request.AdminUserId,
                        VerifiedAt = DateTime.UtcNow
                    };
                    _context.SellerProfiles.Add(sellerProfile);
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
