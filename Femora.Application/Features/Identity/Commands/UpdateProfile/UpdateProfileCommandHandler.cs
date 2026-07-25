using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Application.Common.Interfaces.Repositories;
using Femora.Application.Features.Identity.Queries.GetProfile;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace Femora.Application.Features.Identity.Commands.UpdateProfile;

public class UpdateProfileCommandHandler(IAppDbContext db, IBlobStorageRepository blobStorage)
    : IRequestHandler<UpdateProfileCommand, ProfileDto>
{
    public async Task<ProfileDto> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await db.ApplicationUsers
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken)
            ?? throw new NotFoundException("ApplicationUser", request.UserId.ToString());

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.PhoneNumber = request.PhoneNumber;
        user.Bio = request.Bio;
        user.LinkedInUrl = request.LinkedInUrl;
        user.GitHubUrl = request.GitHubUrl;
        user.Country = request.Country;

        if (request.Avatar is not null)
        {
            await using var stream = request.Avatar.OpenReadStream();
            var avatarUrl = await blobStorage.UploadFileAsync(
                stream,
                request.Avatar.FileName,
                request.Avatar.ContentType,
                folder: "avatars",
                cancellationToken: cancellationToken);

            user.AvatarUrl = avatarUrl;
        }

        await db.SaveChangesAsync(cancellationToken);

        return new ProfileDto
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            Bio = user.Bio,
            AvatarUrl = user.AvatarUrl,
            LinkedInUrl = user.LinkedInUrl,
            GitHubUrl = user.GitHubUrl,
            Country = user.Country
        };
    }
}
