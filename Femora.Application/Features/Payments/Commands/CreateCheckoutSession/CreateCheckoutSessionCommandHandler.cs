using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Application.Features.Payments.Common;
using Femora.Domain.Entities.LMS;
using Femora.Domain.Entities.Marketplace;
using Femora.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Femora.Application.Features.Payments.Commands.CreateCheckoutSession;

public class CreateCheckoutSessionCommandHandler(
    IAppDbContext _db,
    IStripeService _stripe,
    ICurrentUserService _currentUser,
    ILogger<CreateCheckoutSessionCommandHandler> _logger)
    : IRequestHandler<CreateCheckoutSessionCommand, CheckoutSessionResult>
{
    public async Task<CheckoutSessionResult> Handle(
        CreateCheckoutSessionCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        var user = await _db.ApplicationUsers
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new NotFoundException("User", userId.ToString());

        // ── Course Payment ──────────────────────────────────────────────────
        if (request.CourseId.HasValue)
        {
            var course = await _db.Courses
                .FirstOrDefaultAsync(c => c.Id == request.CourseId.Value && c.IsPublished, cancellationToken)
                ?? throw new NotFoundException(nameof(Course), request.CourseId.Value.ToString());

            var alreadyEnrolled = await _db.Enrollments.AnyAsync(
                e => e.CourseId == course.Id
                  && e.TraineeProfile.UserId == userId,
                cancellationToken);

            if (alreadyEnrolled)
                throw new InvalidOperationException("You are already enrolled in this course.");

            var items = new List<PaymentLineItem>
            {
                new(
                    Name: course.Title,
                    Description: course.Description?[..Math.Min(200, course.Description.Length)],
                    UnitPrice: course.Price,
                    Quantity: 1,
                    ItemType: PaymentItemType.Course,
                    ItemId: course.Id)
            };

            return await _stripe.CreateCheckoutSessionAsync(new CreateCheckoutSessionRequest
            {
                UserId = userId,
                UserEmail = user.Email!,
                CourseId = course.Id,
                Items = items,
                SuccessUrl = request.SuccessUrl,
                CancelUrl = request.CancelUrl,
                IdempotencyKey = $"course-{course.Id}-user-{userId}"
            }, cancellationToken);
        }

        // ── Cart / Products Payment ─────────────────────────────────────────
        var cart = await _db.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.ProductVariant)
                    .ThenInclude(v => v.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Cart", userId.ToString());

        List<PaymentLineItem> cartItems;

        if (!cart.Items.Any())
        {
            // Try to find an existing pending order for this user (cart may be empty after placing the order)
            var existingOrder = await _db.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.ProductVariant)
                        .ThenInclude(pv => pv.Product)
                .Include(o => o.Payment)
                .Where(o => o.UserId == userId && o.Status == Domain.Enums.OrderStatus.Pending && o.Payment == null)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (existingOrder == null)
            {
                // Log contextual details to aid diagnostics and then throw a domain-specific exception
                _logger.LogWarning("CreateCheckoutSession: user {UserId} cart {CartId} has no items and no pending order. Request: {@Request}",
                    userId, cart.Id, request);
                throw new EmptyCartException();
            }

            // Build payment items from the existing order
            cartItems = existingOrder.OrderItems.Select(oi => new PaymentLineItem(
                Name: $"{oi.ProductVariant.Product.Name} – {oi.ProductVariant.Name}",
                Description: oi.ProductVariant.Product.Description?[..Math.Min(200, oi.ProductVariant.Product.Description?.Length ?? 0)],
                UnitPrice: oi.UnitPrice,
                Quantity: oi.Quantity,
                ItemType: PaymentItemType.Product,
                ItemId: oi.ProductVariant.ProductId,
                VariantId: oi.ProductVariantId)).ToList();

            // Create checkout session for existing order
            return await _stripe.CreateCheckoutSessionAsync(new CreateCheckoutSessionRequest
            {
                UserId = userId,
                UserEmail = user.Email!,
                Items = cartItems,
                OrderId = existingOrder.Id,
                SuccessUrl = request.SuccessUrl,
                CancelUrl = request.CancelUrl,
                IdempotencyKey = $"order-{existingOrder.Id}-user-{userId}"
            }, cancellationToken);
        }

        cartItems = cart.Items.Select(i => new PaymentLineItem(
            Name: $"{i.ProductVariant.Product.Name} – {i.ProductVariant.Name}",
            Description: i.ProductVariant.Product.Description?[..Math.Min(200, i.ProductVariant.Product.Description?.Length ?? 0)],
            UnitPrice: i.ProductVariant.Price,
            Quantity: i.Quantity,
            ItemType: PaymentItemType.Product,
            ItemId: i.ProductVariant.ProductId,
            VariantId: i.ProductVariantId)).ToList();

        // Create an Order before redirecting to Stripe so the payment is explicitly linked to an Order
        var order = new Order
        {
            UserId = userId,
            Status = Domain.Enums.OrderStatus.Pending,
            TotalAmount = 0
        };

        foreach (var ci in cart.Items)
        {
            order.OrderItems.Add(new OrderItem
            {
                ProductVariantId = ci.ProductVariantId,
                Quantity = ci.Quantity,
                UnitPrice = ci.ProductVariant.Price
            });
            order.TotalAmount += ci.Quantity * ci.ProductVariant.Price;
        }

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(cancellationToken);

        return await _stripe.CreateCheckoutSessionAsync(new CreateCheckoutSessionRequest
        {
            UserId = userId,
            UserEmail = user.Email!,
            Items = cartItems,
            OrderId = order.Id,
            SuccessUrl = request.SuccessUrl,
            CancelUrl = request.CancelUrl,
            IdempotencyKey = $"order-{order.Id}-user-{userId}"
        }, cancellationToken);
    }
}
