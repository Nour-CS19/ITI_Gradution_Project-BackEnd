namespace Femora.Application.Features.Payments.Common;

public enum PaymentItemType { Course, Product }

public record PaymentLineItem(
    string Name,
    string? Description,
    decimal UnitPrice,
    int Quantity,
    PaymentItemType ItemType,
    Guid ItemId,
    Guid? VariantId = null);

/// <summary>
/// Request sent to IStripeService.CreateCheckoutSessionAsync.
/// Either populate Items from a cart (products) or pass a single CourseId for course enrollment payment.
/// </summary>
public record CreateCheckoutSessionRequest
{
    public Guid UserId { get; init; }
    public string UserEmail { get; init; } = string.Empty;

    /// <summary>null = pay for cart items (products). Set for direct course purchase.</summary>
    public Guid? CourseId { get; init; }

    /// <summary>Items from the cart – populated only for product/cart checkout.</summary>
    public List<PaymentLineItem> Items { get; init; } = [];

    /// <summary>If present, indicates this checkout session is for an existing Order.
    /// The webhook will mark this order as paid instead of creating a new order from the cart.</summary>
    public Guid? OrderId { get; init; }

    public string SuccessUrl { get; init; } = string.Empty;
    public string CancelUrl { get; init; } = string.Empty;

    /// <summary>Stripe idempotency key — use orderId or cartId.</summary>
    public string? IdempotencyKey { get; init; }
}

public record CheckoutSessionResult(
    string SessionId,
    string SessionUrl,
    Guid? OrderId = null);

public record StripeWebhookResult(
    bool Handled,
    string EventType,
    string? OrderId = null,
    string? EnrollmentId = null);
