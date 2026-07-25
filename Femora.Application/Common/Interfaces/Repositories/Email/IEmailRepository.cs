namespace Femora.Application.Common.Interfaces.Repositories.Email;

public interface IEmailRepository
{
    Task SendOtpAsync(string email, string userName, string otp, CancellationToken cancellationToken = default);
    Task SendPasswordResetAsync(string email, string userName, string resetLink, CancellationToken cancellationToken = default);
    Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
}
