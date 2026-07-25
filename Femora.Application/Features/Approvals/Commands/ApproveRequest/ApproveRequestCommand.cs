using MediatR;

namespace Femora.Application.Features.Approvals.Commands.ApproveRequest;

public record ApproveRequestCommand(Guid RequestId, Guid AdminId) : IRequest<bool>;
