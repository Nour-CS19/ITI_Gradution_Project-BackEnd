using Femora.Application.Common.Interfaces.Repositories.Email;
using Femora.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace Femora.Infrastructure.Repositories.Email;

public class EmailRepository(
    IOptions<EmailOptions> options,
    ILogger<EmailRepository> logger) : IEmailRepository
{
    private readonly EmailOptions _opts = options.Value;

    public async Task SendOtpAsync(
        string email,
        string userName,
        string otp,
        CancellationToken cancellationToken = default)
    {
        var subject  = "كود التحقق من حسابك في Femora";
        var htmlBody = $"""
            <div dir="rtl" style="font-family:Arial,sans-serif;max-width:600px;margin:auto;padding:24px;border:1px solid #F0E6DE;border-radius:12px;background:#FFF;">
              <h2 style="color:#C8956C;text-align:center;">مرحباً {userName}!</h2>
              <p style="color:#3D2314;text-align:center;">كودك لتأكيد حسابك في Femora هو:</p>
              <div style="text-align:center;margin:24px 0;">
                <span style="display:inline-block;font-size:36px;font-weight:bold;letter-spacing:12px;color:#3D2314;background:#FDF0EA;padding:16px 32px;border-radius:12px;border:2px dashed #C8956C;">
                  {otp}
                </span>
              </div>
              <p style="color:#8B6355;text-align:center;font-size:13px;">الكود صالح لمدة <strong>10 دقائق</strong> فقط.</p>
              <p style="color:#A07060;text-align:center;font-size:12px;">إذا لم تطلبي هذا الكود، تجاهلي هذا البريد.</p>
              <hr style="border:none;border-top:1px solid #F0E6DE;margin:24px 0;" />
              <p style="color:#9CA3AF;font-size:12px;text-align:center;">Femora — منصة التعلم والسوق الحرفي</p>
            </div>
            """;

        await SendAsync(email, subject, htmlBody, cancellationToken);
    }

    public async Task SendPasswordResetAsync(
        string email,
        string userName,
        string resetLink,
        CancellationToken cancellationToken = default)
    {
        var subject  = "إعادة تعيين كلمة المرور - Femora";
        var htmlBody = $"""
            <div dir="rtl" style="font-family:Arial,sans-serif;max-width:600px;margin:auto;padding:24px;border:1px solid #F0E6DE;border-radius:12px;background:#FFF;">
              <h2 style="color:#C8956C;text-align:center;">مرحباً {userName}!</h2>
              <p style="color:#3D2314;text-align:center;">وصلنا طلب لإعادة تعيين كلمة المرور الخاصة بحسابك في Femora.</p>
              <div style="text-align:center;margin:24px 0;">
                <a href="{resetLink}" style="display:inline-block;font-size:16px;font-weight:bold;color:#FFF;background:#C8956C;padding:14px 32px;border-radius:12px;text-decoration:none;">
                  إعادة تعيين كلمة المرور
                </a>
              </div>
              <p style="color:#8B6355;text-align:center;font-size:13px;">الرابط صالح لمدة <strong>ساعة واحدة</strong> فقط.</p>
              <p style="color:#A07060;text-align:center;font-size:12px;">إذا لم تطلبي إعادة تعيين كلمة المرور، تجاهلي هذا البريد ولن يتم تغيير أي شيء.</p>
              <hr style="border:none;border-top:1px solid #F0E6DE;margin:24px 0;" />
              <p style="color:#9CA3AF;font-size:12px;text-align:center;">Femora — منصة التعلم والسوق الحرفي</p>
            </div>
            """;

        await SendAsync(email, subject, htmlBody, cancellationToken);
    }

    public async Task SendAsync(
        string to,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        using var client = new SmtpClient(_opts.Host, _opts.Port)
        {
            Credentials = new NetworkCredential(_opts.Username, _opts.Password),
            EnableSsl   = _opts.EnableSsl,
        };

        using var message = new MailMessage
        {
            From       = new MailAddress(_opts.FromEmail, _opts.FromName),
            Subject    = subject,
            Body       = htmlBody,
            IsBodyHtml = true,
        };
        message.To.Add(to);

        try
        {
            await client.SendMailAsync(message, cancellationToken);
            logger.LogInformation("Email sent to {To}: {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send email to {To}: {Subject}", to, subject);
            throw;
        }
    }
}
