using Femora.Domain.Enums;

namespace Femora.Application.Common.Interfaces;
public interface ICurrentUserService
{
    Guid UserId { get; }
}
