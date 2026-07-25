using Femora.Application.Features.Identity.Common.DTOs;
using MediatR;
namespace Femora.Application.Features.Identity.Commands.VerifyOtp;

public sealed record VerifyOtpCommand(string Email, string Otp) : IRequest<SigninResponseDto>;
