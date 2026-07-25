namespace Femora.Application.Features.Identity.Common.Exceptions;

public sealed class AuthenticationException(string message) : Exception(message);
