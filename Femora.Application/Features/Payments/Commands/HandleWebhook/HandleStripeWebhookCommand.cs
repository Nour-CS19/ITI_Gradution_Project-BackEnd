using Femora.Application.Features.Payments.Common;
using MediatR;

namespace Femora.Application.Features.Payments.Commands.HandleWebhook;

public sealed record HandleStripeWebhookCommand(
    string Json,
    string StripeSignatureHeader) : IRequest<StripeWebhookResult>;
