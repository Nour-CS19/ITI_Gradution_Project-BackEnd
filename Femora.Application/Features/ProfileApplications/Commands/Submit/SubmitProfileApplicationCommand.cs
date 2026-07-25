using System;
using System.Threading;
using System.Threading.Tasks;
using Femora.Application.Common.Interfaces;
using Femora.Domain.Entities.Identity;
using Femora.Domain.Enums;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.ProfileApplications.Commands.Submit;

public class SubmitProfileApplicationCommand : IRequest<Guid>
{
    public Guid UserId { get; set; }
    public RequestedRole RequestedRole { get; set; }
    public string Bio { get; set; } = string.Empty;
    public string PortfolioUrl { get; set; } = string.Empty;
    public string NationalIdNumber { get; set; } = string.Empty;
    
    // Seller only fields
    public string? StoreName { get; set; }
    public string? StoreDescription { get; set; }
}

public class SubmitProfileApplicationCommandValidator : AbstractValidator<SubmitProfileApplicationCommand>
{
    public SubmitProfileApplicationCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.RequestedRole).IsInEnum();
        RuleFor(x => x.Bio).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.PortfolioUrl).NotEmpty().MaximumLength(500);
        RuleFor(x => x.NationalIdNumber).NotEmpty().MaximumLength(50);

        RuleFor(x => x.StoreName)
            .NotEmpty().When(x => x.RequestedRole == RequestedRole.Seller)
            .MaximumLength(200);

        RuleFor(x => x.StoreDescription)
            .NotEmpty().When(x => x.RequestedRole == RequestedRole.Seller)
            .MaximumLength(1000);
    }
}

public class SubmitProfileApplicationCommandHandler : IRequestHandler<SubmitProfileApplicationCommand, Guid>
{
    private readonly IAppDbContext _context;

    public SubmitProfileApplicationCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> Handle(SubmitProfileApplicationCommand request, CancellationToken cancellationToken)
    {
        // 1. Check if user already has the target profile
        if (request.RequestedRole == RequestedRole.Instructor)
        {
            var hasProfile = await _context.InstructorProfiles.AnyAsync(p => p.UserId == request.UserId, cancellationToken);
            if (hasProfile)
                throw new InvalidOperationException("You already have an Instructor profile.");
        }
        else if (request.RequestedRole == RequestedRole.Seller)
        {
            var hasProfile = await _context.SellerProfiles.AnyAsync(p => p.UserId == request.UserId, cancellationToken);
            if (hasProfile)
                throw new InvalidOperationException("You already have a Seller profile.");
        }

        // 2. Check for active pending request for same UserId + RequestedRole
        var hasPending = await _context.ProfileApplicationRequests.AnyAsync(r =>
            r.UserId == request.UserId &&
            r.RequestedRole == request.RequestedRole &&
            r.Status == ApplicationRequestStatus.Pending,
            cancellationToken);

        if (hasPending)
            throw new InvalidOperationException("A pending application for this role already exists.");

        // 3. Create request
        var application = new ProfileApplicationRequest
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            RequestedRole = request.RequestedRole,
            Status = ApplicationRequestStatus.Pending,
            SubmittedAt = DateTime.UtcNow,
            Bio = request.Bio,
            PortfolioUrl = request.PortfolioUrl,
            NationalIdNumber = request.NationalIdNumber,
            StoreName = request.RequestedRole == RequestedRole.Seller ? request.StoreName : null,
            StoreDescription = request.RequestedRole == RequestedRole.Seller ? request.StoreDescription : null
        };

        _context.ProfileApplicationRequests.Add(application);
        await _context.SaveChangesAsync(cancellationToken);

        return application.Id;
    }
}
