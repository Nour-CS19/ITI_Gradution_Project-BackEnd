using Femora.Application.Features.Payments.Common;
using MediatR;

namespace Femora.Application.Features.Payments.Commands.CreateCheckoutSession;

/// <summary>
/// Creates a Stripe Checkout Session for either:
/// - A single course (CourseId set, Items empty)
/// - Products from the cart (CourseId null, Items populated from cart)
/// </summary>
public sealed record CreateCheckoutSessionCommand(
    Guid? CourseId,
    string SuccessUrl,
    string CancelUrl) : IRequest<CheckoutSessionResult>;
