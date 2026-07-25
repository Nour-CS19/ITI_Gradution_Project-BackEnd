using Femora.Application.Common.Interfaces;
using Femora.Application.Features.Payments.Common;
using MediatR;

namespace Femora.Application.Features.Payments.Commands.HandleWebhook;

public class HandleStripeWebhookCommandHandler(IStripeService _stripe)
    : IRequestHandler<HandleStripeWebhookCommand, StripeWebhookResult>
{
    public Task<StripeWebhookResult> Handle(
        HandleStripeWebhookCommand request,
        CancellationToken cancellationToken)
        => _stripe.HandleWebhookAsync(request.Json, request.StripeSignatureHeader, cancellationToken);
}
