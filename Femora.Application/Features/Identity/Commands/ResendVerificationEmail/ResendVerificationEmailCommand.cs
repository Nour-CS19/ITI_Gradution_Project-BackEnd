using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Application.Common.Interfaces.Repositories.Email;
using Femora.Domain.Entities;
using Femora.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.Identity.Commands.ResendVerificationEmail;

public sealed record ResendVerificationEmailCommand(string Email) : IRequest;

public class ResendVerificationEmailCommandHandler(
    UserManager<ApplicationUser> _userManager,
    IAppDbContext _context,
    IEmailRepository _emailRepository)
    : IRequestHandler<ResendVerificationEmailCommand>
{
    public async Task Handle(ResendVerificationEmailCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new NotFoundException("User", request.Email);

        if (user.EmailConfirmed)
            throw new InvalidOperationException("Email is already verified.");

        // Invalidate old OTPs
        var oldOtps = await _context.EmailOtps
            .Where(o => o.UserId == user.Id && !o.IsUsed)
            .ToListAsync(cancellationToken);
        foreach (var old in oldOtps) old.IsUsed = true;

        // Generate new OTP
        var code = Random.Shared.Next(100_000, 999_999).ToString();
        await _context.EmailOtps.AddAsync(new EmailOtp
        {
            UserId    = user.Id,
            Code      = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
        }, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        await _emailRepository.SendOtpAsync(
            user.Email!,
            $"{user.FirstName} {user.LastName}",
            code,
            cancellationToken);
    }
}
