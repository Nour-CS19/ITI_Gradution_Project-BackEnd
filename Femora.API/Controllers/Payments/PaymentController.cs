using Femora.Application.Features.Payments.Commands.CreateCheckoutSession;
using Femora.Application.Features.Payments.Commands.HandleWebhook;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Femora.API.Controllers.Payments;

[Route("api/payments")]
[ApiController]
public class PaymentController(IMediator mediator, ILogger<PaymentController> logger) : ControllerBase
{
    /// <summary>
    /// Creates a Stripe Checkout Session.
    ///
    /// - To pay for a single course: set courseId; leave successUrl / cancelUrl to your frontend.
    /// - To pay for cart (products): omit courseId – items are read from the authenticated user's cart.
    ///
    /// Returns { sessionId, sessionUrl } — redirect the user to sessionUrl to complete payment.
    /// </summary>
    [HttpPost("checkout")]
    [Authorize]
    public async Task<IActionResult> CreateCheckoutSession(
        [FromBody] CreateCheckoutSessionCommand command,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(command, cancellationToken);
        // If this request comes from a regular browser navigation (not an XHR/fetch expecting JSON)
        // redirect the user directly to the Stripe hosted checkout page. If the client is an API
        // caller (Accept: application/json or X-Requested-With: XMLHttpRequest) return JSON.
        var accept = Request.Headers["Accept"].ToString();
        var isAjax = Request.Headers.TryGetValue("X-Requested-With", out var xrw) && xrw == "XMLHttpRequest";

        if (!isAjax && !accept.Contains("application/json", StringComparison.OrdinalIgnoreCase))
        {
            // Redirect to the Stripe session URL so the browser navigates to the payment gateway.
            if (!string.IsNullOrEmpty(result.SessionUrl))
                return Redirect(result.SessionUrl);
        }

        // For API/AJAX callers return session info including orderId when available
        return Ok(new
        {
            sessionId = result.SessionId,
            sessionUrl = result.SessionUrl,
            orderId = result.OrderId
        });
    }

    /// <summary>
    /// Stripe webhook endpoint.
    /// Must be excluded from [Authorize] — Stripe signs the request instead.
    /// Configure in Stripe dashboard: POST https://yourdomain.com/api/payments/webhook
    /// </summary>
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook(CancellationToken cancellationToken)
    {
        Request.EnableBuffering();
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync(cancellationToken);
        HttpContext.Request.Body.Seek(0, SeekOrigin.Begin);
        logger.LogInformation("Webhook payload length: {Length}", json?.Length ?? 0);
        //logger.LogInformation("Stripe webhook received. Payload length: {Length} characters.", json?.Length ?? 0);

        if (!Request.Headers.TryGetValue("Stripe-Signature", out var stripeSignature))
            return BadRequest("Missing Stripe-Signature header.");

        try
        {
            var result = await mediator.Send(
                new HandleStripeWebhookCommand(json, stripeSignature.ToString()),
                cancellationToken);

            return Ok(new { handled = result.Handled, eventType = result.EventType });
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Stripe Webhook Signature Verification failed: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
    }
}
