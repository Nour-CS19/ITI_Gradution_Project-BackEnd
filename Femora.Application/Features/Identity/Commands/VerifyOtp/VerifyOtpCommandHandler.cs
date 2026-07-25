using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Extensions;
using Femora.Application.Common.Interfaces;
using Femora.Application.Features.Identity.Common.DTOs;
using Femora.Application.Features.Identity.Common.Exceptions;
using Femora.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Femora.Application.Features.Identity.Commands.VerifyOtp;

public class VerifyOtpCommandHandler(
    UserManager<ApplicationUser> _userManager,
    IAppDbContext _context,
    ITokenService _tokenService)
    : IRequestHandler<VerifyOtpCommand, SigninResponseDto>
{
    public async Task<SigninResponseDto> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email)
            ?? throw new NotFoundException("User", request.Email);

        if (user.EmailConfirmed)
            return await BuildResponseAsync(user, cancellationToken);

        var otp = await _context.EmailOtps
            .Where(o => o.UserId == user.Id && !o.IsUsed && o.Code == request.Otp)
            .OrderByDescending(o => o.ExpiresAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (otp is null)
            throw new InvalidTokenException("الكود غير صحيح. تأكدي من الكود المرسل.");

        if (otp.IsExpired)
            throw new InvalidTokenException("انتهت صلاحية الكود. اطلبي كودًا جديدًا.");

        // Mark OTP used + confirm email
        otp.IsUsed = true;
        user.EmailConfirmed = true;
        await _userManager.UpdateAsync(user);
        await _context.SaveChangesAsync(cancellationToken);

        return await BuildResponseAsync(user, cancellationToken);
    }

    private async Task<SigninResponseDto> BuildResponseAsync(ApplicationUser user, CancellationToken ct)
    {
        var auth = new AuthResponseDto
        {
            User         = await user.ToUserDtoAsync(_userManager),
            AccessToken  = await _tokenService.GenerateAccessTokenAsync(user.Id, null),
            RefreshToken = await _tokenService.GenerateRefreshTokenAsync(user.Id, null),
            ExpiresAt    = DateTime.UtcNow.AddHours(1),
        };
        await _context.SaveChangesAsync(ct);
        return new SigninResponseDto { RequiresProfileSelection = false, Auth = auth };
    }
}
