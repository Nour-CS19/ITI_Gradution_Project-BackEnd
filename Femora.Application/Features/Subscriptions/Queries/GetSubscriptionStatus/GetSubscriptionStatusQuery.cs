using MediatR;
using Femora.Application.Features.Subscriptions.Common.DTOs;

namespace Femora.Application.Features.Subscriptions.Queries.GetSubscriptionStatus;

public class GetSubscriptionStatusQuery : IRequest<SubscriptionStatusDto?>
{
    public Guid UserId { get; set; }
}
