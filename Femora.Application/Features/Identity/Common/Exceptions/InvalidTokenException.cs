namespace Femora.Application.Features.Identity.Common.Exceptions;

public sealed class InvalidTokenException(string message) : Exception(message);
