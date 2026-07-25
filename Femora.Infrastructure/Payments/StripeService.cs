using Femora.Application.Common.Exceptions;
using Femora.Application.Common.Interfaces;
using Femora.Application.Features.Payments.Common;
using Femora.Domain.Entities.LMS;
using Femora.Domain.Entities.Marketplace;
using Femora.Domain.Enums;
using Femora.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.Checkout;

namespace Femora.Infrastructure.Payments;

public class StripeService(
    IOptions<StripeOptions> _options,
    IAppDbContext _db,
    IOnboardingProfileSyncService _onboardingProfileSync,
    ILogger<StripeService> _logger,
    IConfiguration _configuration)
    : IStripeService
{
    private const long CentsFactor = 100L; // Stripe amounts are in cents

    // ─── Create Checkout Session ────────────────────────────────────────────

    public async Task<CheckoutSessionResult> CreateCheckoutSessionAsync(
        CreateCheckoutSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        StripeConfiguration.ApiKey = _options.Value.SecretKey;

        var lineItems = request.Items.Select(i => new SessionLineItemOptions
        {
            PriceData = new SessionLineItemPriceDataOptions
            {
                Currency = _options.Value.Currency,
                UnitAmount = (long)(i.UnitPrice * CentsFactor),
                ProductData = new SessionLineItemPriceDataProductDataOptions
                {
                    Name = i.Name,
                    Description = i.Description,
                }
            },
            Quantity = i.Quantity
        }).ToList();

        // Metadata lets us identify what was paid for in the webhook
        var metadata = new Dictionary<string, string>
        {
            ["userId"] = request.UserId.ToString(),
        };

        if (request.CourseId.HasValue)
        {
            metadata["type"] = "course";
            metadata["courseId"] = request.CourseId.Value.ToString();
        }
        else if (request.OrderId.HasValue)
        {
            metadata["type"] = "order";
            metadata["orderId"] = request.OrderId.Value.ToString();
        }
        else
        {
            metadata["type"] = "cart";
            // Encode first 5 item IDs so webhook can confirm cart items
            var ids = string.Join(",", request.Items.Select(i => $"{i.ItemId}:{i.VariantId}:{i.Quantity}"));
            metadata["cartItems"] = ids[..Math.Min(500, ids.Length)];
        }

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = ["card"],
            Mode = "payment",
            CustomerEmail = request.UserEmail,
            LineItems = lineItems,
            SuccessUrl = request.SuccessUrl,
            CancelUrl = request.CancelUrl,
            Metadata = metadata,
            // Use Stripe's automatic locale detection to avoid invalid locale errors
            // (Stripe accepts only a fixed set of locales; "auto" lets Stripe pick)
            Locale = "auto",
            PaymentIntentData = new SessionPaymentIntentDataOptions
            {
                Metadata = metadata   // also on the PaymentIntent for webhook lookup
            }
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options, cancellationToken: cancellationToken);

        _logger.LogInformation("Stripe checkout session {SessionId} created for user {UserId}", session.Id, request.UserId);

        // Return orderId if caller supplied one (orders created before redirect)
        return new CheckoutSessionResult(session.Id, session.Url, request.OrderId);
    }

    // ─── Handle Webhook ────────────────────────────────────────────────────

    public async Task<StripeWebhookResult> HandleWebhookAsync(
        string json,
        string stripeSignatureHeader,
        CancellationToken cancellationToken = default)
    {
        StripeConfiguration.ApiKey = _options.Value.SecretKey;

        Event stripeEvent;
        var enableBypass = _configuration.GetValue<bool>("Payments:EnableWebhookSignatureBypass");
        var devBypassSignature = _configuration.GetValue<string>("Payments:DevBypassSignature");

        if (enableBypass && !string.IsNullOrEmpty(devBypassSignature)
                    && stripeSignatureHeader == devBypassSignature)
        {
            stripeEvent = EventUtility.ParseEvent(json);
            _logger.LogWarning("SECURITY ALERT: Dev bypass triggered.");
        }
        else
        {

    


            try
            {
                stripeEvent = EventUtility.ConstructEvent(
json,
stripeSignatureHeader,
_options.Value.WebhookSecret,
throwOnApiVersionMismatch: false);
            }
            catch (StripeException ex)
            {
                _logger.LogWarning(ex, "Stripe webhook signature validation failed");
                throw new InvalidOperationException("Invalid Stripe webhook signature.", ex);
            }
        }

        _logger.LogInformation("Stripe webhook received: {EventType}", stripeEvent.Type);
            if (stripeEvent.Type != "checkout.session.completed")
            return new StripeWebhookResult(Handled: false, EventType: stripeEvent.Type);

        var session = stripeEvent.Data.Object as Session
            ?? throw new InvalidOperationException("Webhook event data is not a Session.");

        var meta = session.Metadata;

        if (!meta.TryGetValue("userId", out var userIdStr) || !Guid.TryParse(userIdStr, out var userId))
        {
            _logger.LogError("Webhook session {SessionId} missing valid userId metadata", session.Id);
            return new StripeWebhookResult(Handled: false, EventType: stripeEvent.Type);
        }

        if (!meta.TryGetValue("type", out var paymentType))
            return new StripeWebhookResult(Handled: false, EventType: stripeEvent.Type);

        if (paymentType == "course" && meta.TryGetValue("courseId", out var courseIdStr) && Guid.TryParse(courseIdStr, out var courseId))
        {
            var enrollmentId = await FulfillCourseEnrollmentAsync(userId, courseId, session, cancellationToken);
            return new StripeWebhookResult(Handled: true, EventType: stripeEvent.Type, EnrollmentId: enrollmentId?.ToString());
        }

        if (paymentType == "order" && meta.TryGetValue("orderId", out var orderIdStr) && Guid.TryParse(orderIdStr, out var orderId))
        {
            var paidOrderId = await FulfillOrderAsync(userId, orderId, session, cancellationToken);
            return new StripeWebhookResult(Handled: true, EventType: stripeEvent.Type, OrderId: paidOrderId?.ToString());
        }

        // fallback for older sessions created with cart metadata
        if (paymentType == "cart")
        {
            var cartOrderId = await FulfillCartOrderAsync(userId, session, cancellationToken);
            return new StripeWebhookResult(Handled: true, EventType: stripeEvent.Type, OrderId: cartOrderId?.ToString());
        }

        return new StripeWebhookResult(Handled: false, EventType: stripeEvent.Type);
    }

    // ─── Course Fulfillment ────────────────────────────────────────────────

    private async Task<Guid?> FulfillCourseEnrollmentAsync(
        Guid userId, Guid courseId, Session session,
        CancellationToken cancellationToken)
    {
        using var transaction = await _db.BeginTransactionAsync(cancellationToken);
        try
        {
            // The webhook is the first confirmed enrollment moment for paid courses.
            // Create the TraineeProfile here only after Stripe confirms payment, then
            // copy the registration goal/preferences into the trainee tables.
            var profileSync = await _onboardingProfileSync.EnsureTraineeProfileAsync(userId, cancellationToken);
            var traineeProfileId = profileSync.TraineeProfileId;

            // Idempotency: skip if already enrolled
            var alreadyEnrolled = await _db.Enrollments.AnyAsync(
                e => e.CourseId == courseId && e.TraineeProfileId == traineeProfileId,
                cancellationToken);

            if (alreadyEnrolled)
            {
                _logger.LogWarning("Duplicate webhook: user {UserId} already enrolled in course {CourseId}", userId, courseId);
                return null;
            }

            var course = await _db.Courses
                .Include(c => c.Modules).ThenInclude(m => m.Lessons)
                .FirstOrDefaultAsync(c => c.Id == courseId, cancellationToken)
                ?? throw new NotFoundException(nameof(Course), courseId.ToString());

            var enrollment = new Enrollment
            {
                TraineeProfileId = traineeProfileId,
                CourseId = courseId,
                PricePaid = course.Price,
                EnrolledAt = DateTime.UtcNow,
            };
            _db.Enrollments.Add(enrollment);
            await _db.SaveChangesAsync(cancellationToken);

            // Lesson progresses
            var lessonProgresses = course.Modules
                .SelectMany(m => m.Lessons)
                .Select(l => new LessonProgress
                {
                    EnrollmentId = enrollment.Id,
                    LessonId = l.Id,
                    IsCompleted = false,
                    WatchedSeconds = 0
                }).ToList();
            await _db.LessonProgresses.AddRangeAsync(lessonProgresses, cancellationToken);

            // Enrollment modules — unlock first
            var orderedModules = course.Modules.OrderBy(m => m.OrderIndex).ToList();
            var enrollmentModules = orderedModules.Select((m, idx) => new EnrollmentModule
            {
                ModuleId = m.Id,
                EnrollmentId = enrollment.Id,
                IsUnlocked = idx == 0  // first module unlocked immediately
            }).ToList();
            await _db.EnrollmentModules.AddRangeAsync(enrollmentModules, cancellationToken);

            // Instructor earning
            _db.InstructorEarnings.Add(new InstructorEarning
            {
                InstructorProfileId = course.InstructorProfileId,
                EnrollmentId = enrollment.Id,
                Amount = course.Price,
                PlatformFee = InstructorEarning.CalculatePlatformFee(course.Price),
                Status = EarningStatus.Pending,
                EarnedAt = DateTime.UtcNow
            });

            // Payment record (for course purchases we reference the enrollment)
            _db.Payments.Add(new Payment
            {
                UserId = userId,
                EnrollmentId = enrollment.Id,
                Amount = course.Price,
                PaymentMethod = "stripe",
                PaymentStatus = "paid",
                TransactionReference = session.PaymentIntentId ?? session.Id,
                PaidAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("User {UserId} enrolled in course {CourseId} after Stripe payment", userId, courseId);
            return enrollment.Id;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    // ─── Cart / Order Fulfillment ──────────────────────────────────────────

    private async Task<Guid?> FulfillCartOrderAsync(
        Guid userId, Session session,
        CancellationToken cancellationToken)
    {
        var cart = await _db.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.ProductVariant)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (cart is null || !cart.Items.Any())
        {
            _logger.LogWarning("Webhook for cart order: cart empty or not found for user {UserId}", userId);
            return null;
        }

        var order = new Order
        {
            UserId = userId,
            Status = OrderStatus.Processing
        };

        foreach (var item in cart.Items)
        {
            order.OrderItems.Add(new OrderItem
            {
                ProductVariantId = item.ProductVariantId,
                Quantity = item.Quantity,
                UnitPrice = item.ProductVariant.Price
            });
            order.TotalAmount += item.Quantity * item.ProductVariant.Price;
        }

        _db.Orders.Add(order);
        await _db.SaveChangesAsync(cancellationToken);

        // Payment record
        _db.Payments.Add(new Payment
        {
            UserId = userId,
            OrderId = order.Id,
            Amount = order.TotalAmount,
            PaymentMethod = "stripe",
            PaymentStatus = "paid",
            TransactionReference = session.PaymentIntentId ?? session.Id,
            PaidAt = DateTime.UtcNow
        });

        // Clear cart — bulk delete by CartId instead of RemoveRange(cart.Items): if the
        // buyer had already removed an item themselves right as this webhook fired,
        // RemoveRange+SaveChanges would throw DbUpdateConcurrencyException on that
        // now-missing row and this whole handler (including the order/payment we just
        // created) would fail. ExecuteDeleteAsync just deletes whatever still matches.
        await _db.CartItems
            .Where(i => i.CartId == cart.Id)
            .ExecuteDeleteAsync(cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderId} created for user {UserId} after Stripe payment", order.Id, userId);
        return order.Id;
    }

    // Fulfill an order that was created before checkout (OrderId included in Stripe session metadata)
    private async Task<Guid?> FulfillOrderAsync(
        Guid userId, Guid orderId, Session session,
        CancellationToken cancellationToken)
    {
        var order = await _db.Orders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order is null)
        {
            _logger.LogWarning("Webhook referenced non-existent order {OrderId} for user {UserId}", orderId, userId);
            return null;
        }

        if (order.UserId != userId)
        {
            _logger.LogWarning("Webhook order {OrderId} user mismatch: expected {OrderUser}, webhook {WebhookUser}", order.UserId, userId);
            return null;
        }

        // Idempotency: if payment already recorded or order already processed, skip
        if (order.Status != OrderStatus.Pending)
        {
            _logger.LogInformation("Order {OrderId} already processed with status {Status}", order.Id, order.Status);
            return order.Id;
        }

        // Mark order as processing/paid and record payment
        order.Status = OrderStatus.Processing;

        _db.Payments.Add(new Payment
        {
            UserId = userId,
            OrderId = order.Id,
            Amount = order.TotalAmount,
            PaymentMethod = "stripe",
            PaymentStatus = "paid",
            TransactionReference = session.PaymentIntentId ?? session.Id,
            PaidAt = DateTime.UtcNow
        });

        // Clear cart items for this user
        var cart = await _db.Carts.FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);
        if (cart != null)
        {
            await _db.CartItems.Where(i => i.CartId == cart.Id).ExecuteDeleteAsync(cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderId} marked paid for user {UserId} after Stripe webhook", order.Id, userId);
        return order.Id;
    }
}
