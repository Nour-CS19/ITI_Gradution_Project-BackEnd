using Femora.Application.Features.Payments.Common;

namespace Femora.Application.Common.Interfaces;

public interface IStripeService
{
    /// <summary>Creates a Stripe Checkout Session for a cart (products) or a single course.</summary>
    Task<CheckoutSessionResult> CreateCheckoutSessionAsync(
        CreateCheckoutSessionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>Constructs and validates the Stripe webhook event from the raw body + signature.</summary>
    Task<StripeWebhookResult> HandleWebhookAsync(
        string json,
        string stripeSignatureHeader,
        CancellationToken cancellationToken = default);
}
