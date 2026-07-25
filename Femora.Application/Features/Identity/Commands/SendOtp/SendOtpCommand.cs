using MediatR;
namespace Femora.Application.Features.Identity.Commands.SendOtp;

public sealed record SendOtpCommand(string Email) : IRequest;
