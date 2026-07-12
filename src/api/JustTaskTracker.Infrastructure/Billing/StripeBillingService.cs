using JustTaskTracker.Application.Billing.Abstractions;
using JustTaskTracker.Domain.Billing.DTOs;
using JustTaskTracker.Infrastructure.Common.Options;
using Stripe;
using Stripe.Checkout;

namespace JustTaskTracker.Infrastructure.Billing;

internal class StripeBillingService(
    IStripeClient stripeClient,
    StripeOptions stripeOptions,
    IPlanCatalog planCatalog) : IBillingService
{
    private const string MetadataUserIdKey = "userId";
    private const string MetadataPlanIdKey = "planId";

    public async Task<CheckoutSessionResult> CreateCheckoutSessionAsync(
        Guid userId,
        string email,
        string planId,
        string? stripeCustomerId = null,
        CancellationToken ct = default)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(userId, Guid.Empty);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(planId);

        var priceId = planCatalog.GetPriceId(planId);
        var userIdValue = userId.ToString("D");

        var options = new SessionCreateOptions
        {
            Mode = "subscription",
            SuccessUrl = stripeOptions.SuccessUrl,
            CancelUrl = stripeOptions.CancelUrl,
            ClientReferenceId = userIdValue,
            LineItems =
            [
                new SessionLineItemOptions
                {
                    Price = priceId,
                    Quantity = 1,
                },
            ],
            Metadata = new Dictionary<string, string>
            {
                [MetadataUserIdKey] = userIdValue,
                [MetadataPlanIdKey] = planId,
            },
            SubscriptionData = new SessionSubscriptionDataOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    [MetadataUserIdKey] = userIdValue,
                    [MetadataPlanIdKey] = planId,
                },
            },
        };

        if (!string.IsNullOrWhiteSpace(stripeCustomerId))
            options.Customer = stripeCustomerId;
        else
            options.CustomerEmail = email;

        var sessionService = new SessionService(stripeClient);
        var session = await sessionService.CreateAsync(options, cancellationToken: ct);

        if (string.IsNullOrWhiteSpace(session.Url))
            throw new InvalidOperationException("Stripe Checkout Session was created without a URL.");

        return new CheckoutSessionResult(session.Id, session.Url);
    }

    public async Task<string> CreateCustomerPortalSessionAsync(
        string stripeCustomerId,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stripeCustomerId);

        var options = new Stripe.BillingPortal.SessionCreateOptions
        {
            Customer = stripeCustomerId,
            ReturnUrl = stripeOptions.CancelUrl,
        };

        var portalService = new Stripe.BillingPortal.SessionService(stripeClient);
        var session = await portalService.CreateAsync(options, cancellationToken: ct);

        if (string.IsNullOrWhiteSpace(session.Url))
            throw new InvalidOperationException("Stripe Customer Portal Session was created without a URL.");

        return session.Url;
    }

    public Task<BillingWebhookEvent> ParseWebhookEventAsync(
        string payload,
        string stripeSignatureHeader,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(payload);
        ArgumentException.ThrowIfNullOrWhiteSpace(stripeSignatureHeader);
        ct.ThrowIfCancellationRequested();

        var stripeEvent = EventUtility.ConstructEvent(
            payload,
            stripeSignatureHeader,
            stripeOptions.WebhookSecret);

        return Task.FromResult(MapWebhookEvent(stripeEvent));
    }

    private static BillingWebhookEvent MapWebhookEvent(Event stripeEvent)
    {
        return stripeEvent.Data.Object switch
        {
            Subscription subscription => MapFromSubscription(stripeEvent, subscription),
            Session checkoutSession => MapFromCheckoutSession(stripeEvent, checkoutSession),
            _ => new BillingWebhookEvent(
                stripeEvent.Id,
                stripeEvent.Type,
                StripeCustomerId: null,
                StripeSubscriptionId: null,
                StripePriceId: null,
                PlanId: null,
                Status: null,
                CurrentPeriodStartUtc: null,
                CurrentPeriodEndUtc: null,
                CancelAtPeriodEnd: false,
                UserId: null),
        };
    }

    private static BillingWebhookEvent MapFromSubscription(Event stripeEvent, Subscription subscription)
    {
        var items = subscription.Items?.Data ?? [];
        var priceId = items
            .Select(item => item.Price?.Id)
            .FirstOrDefault(id => !string.IsNullOrWhiteSpace(id));

        return new BillingWebhookEvent(
            stripeEvent.Id,
            stripeEvent.Type,
            subscription.CustomerId,
            subscription.Id,
            priceId,
            TryGetMetadataValue(subscription.Metadata, MetadataPlanIdKey),
            subscription.Status,
            ToUtc(items.Select(item => item.CurrentPeriodStart).DefaultIfEmpty().Min()),
            ToUtc(items.Select(item => item.CurrentPeriodEnd).DefaultIfEmpty().Max()),
            subscription.CancelAtPeriodEnd,
            TryGetUserId(subscription.Metadata));
    }

    private static BillingWebhookEvent MapFromCheckoutSession(Event stripeEvent, Session session)
    {
        return new BillingWebhookEvent(
            stripeEvent.Id,
            stripeEvent.Type,
            session.CustomerId,
            session.SubscriptionId,
            StripePriceId: null,
            TryGetMetadataValue(session.Metadata, MetadataPlanIdKey),
            Status: session.Status,
            CurrentPeriodStartUtc: null,
            CurrentPeriodEndUtc: null,
            CancelAtPeriodEnd: false,
            TryGetUserId(session.Metadata) ?? TryParseGuid(session.ClientReferenceId));
    }

    private static Guid? TryGetUserId(IDictionary<string, string>? metadata)
    {
        var value = TryGetMetadataValue(metadata, MetadataUserIdKey);

        return TryParseGuid(value);
    }

    private static string? TryGetMetadataValue(IDictionary<string, string>? metadata, string key)
    {
        if (metadata is null || !metadata.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
            return null;

        return value;
    }

    private static Guid? TryParseGuid(string? value) =>
        Guid.TryParse(value, out var userId) ? userId : null;

    private static DateTime? ToUtc(DateTime value) =>
        value == default ? null : value;
}
