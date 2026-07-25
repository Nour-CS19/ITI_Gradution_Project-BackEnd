namespace Femora.Application.Common.Exceptions;

public class NotFoundException(string entity, string id) : Exception($"The {entity} with id: '{id}' not found");