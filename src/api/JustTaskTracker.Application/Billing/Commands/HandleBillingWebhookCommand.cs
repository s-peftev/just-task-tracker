using FluentValidation;
using JustTaskTracker.Application.Billing.Abstractions;
using JustTaskTracker.Application.Billing.Repositories;
using JustTaskTracker.Application.Billing.Webhooks;
using JustTaskTracker.Application.Common.Persistence;
using JustTaskTracker.Application.Common.Utils;
using JustTaskTracker.Domain.Billing.Constants;
using JustTaskTracker.Domain.Billing.DTOs;
using JustTaskTracker.Domain.Billing.Entities;
using JustTaskTracker.Domain.Common.Enums;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;
using Microsoft.Extensions.Logging;

namespace JustTaskTracker.Application.Billing.Commands;

public record HandleBillingWebhookCommand(string Payload, string StripeSignature) : IRequest<Result>;

public class HandleBillingWebhookCommandHandler(
    IBillingService billingService,
    IEnumerable<IBillingWebhookEventHandler> eventHandlers,
    IStripeWebhookEventRepository webhookEventRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ILogger<HandleBillingWebhookCommandHandler> logger)
    : IRequestHandler<HandleBillingWebhookCommand, Result>
{
    public async Task<Result> Handle(HandleBillingWebhookCommand request, CancellationToken ct)
    {
        BillingWebhookEvent billingEvent;

        try
        {
            billingEvent = await billingService.ParseWebhookEventAsync(
                request.Payload,
                request.StripeSignature,
                ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Rejected Stripe webhook due to invalid signature or payload.");
            return Result.Failure(GeneralErrors.InvalidRequest);
        }

        var webhookEvent = await webhookEventRepository.GetByEventIdAsync(billingEvent.EventId, ct);

        if (webhookEvent is { ProcessedAtUtc: not null })
            return Result.Success();

        webhookEvent ??= RegisterNewEvent(billingEvent);

        var handler = eventHandlers.FirstOrDefault(h =>
            h.EventType.Equals(billingEvent.EventType, StringComparison.Ordinal));

        if (handler is null)
        {
            logger.LogInformation(
                "No handler registered for Stripe event type '{EventType}'. Acknowledging event {EventId}.",
                billingEvent.EventType,
                billingEvent.EventId);

            MarkProcessed(webhookEvent);
            await unitOfWork.SaveChangesAsync(ct);

            return Result.Success();
        }

        try
        {
            var handlerResult = await handler.HandleAsync(billingEvent, ct);

            if (handlerResult.IsSuccess)
            {
                MarkProcessed(webhookEvent);
                await unitOfWork.SaveChangesAsync(ct);

                return Result.Success();
            }

            // Permanent business/validation failures are persisted and ACKed so Stripe does not retry forever.
            if (IsPermanentFailure(handlerResult.Error.Type))
            {
                MarkFailed(webhookEvent, handlerResult.Error);
                await unitOfWork.SaveChangesAsync(ct);

                logger.LogWarning(
                    "Stripe webhook {EventId} ({EventType}) failed permanently: {ErrorCode}.",
                    billingEvent.EventId,
                    billingEvent.EventType,
                    handlerResult.Error.Code);

                return Result.Success();
            }

            return handlerResult;
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Transient failure while processing Stripe webhook {EventId} ({EventType}).",
                billingEvent.EventId,
                billingEvent.EventType);

            return Result.Failure(GeneralErrors.ServiceUnavailable);
        }
    }

    private StripeWebhookEvent RegisterNewEvent(BillingWebhookEvent billingEvent)
    {
        var webhookEvent = new StripeWebhookEvent
        {
            EventId = billingEvent.EventId,
            EventType = billingEvent.EventType,
            ReceivedAtUtc = dateTimeProvider.UtcNow,
        };

        webhookEventRepository.Add(webhookEvent);

        return webhookEvent;
    }

    private void MarkProcessed(StripeWebhookEvent webhookEvent)
    {
        webhookEvent.ProcessedAtUtc = dateTimeProvider.UtcNow;
        webhookEvent.LastError = null;
    }

    private void MarkFailed(StripeWebhookEvent webhookEvent, Error error)
    {
        webhookEvent.ProcessedAtUtc = dateTimeProvider.UtcNow;
        webhookEvent.LastError = TruncateError(FormatError(error));
    }

    private static bool IsPermanentFailure(ErrorType type) =>
        type is ErrorType.Validation or ErrorType.NotFound or ErrorType.Conflict or ErrorType.Business;

    private static string FormatError(Error error)
    {
        if (error.Details is { } details && details.Any())
            return $"{error.Code}: {string.Join("; ", details)}";

        return error.Code;
    }

    private static string TruncateError(string message) =>
        message.Length <= StripeWebhookEventFieldLengths.MaxLastErrorLength
            ? message
            : message[..StripeWebhookEventFieldLengths.MaxLastErrorLength];
}

public class HandleBillingWebhookCommandValidator : AbstractValidator<HandleBillingWebhookCommand>
{
    public HandleBillingWebhookCommandValidator()
    {
        RuleFor(x => x.Payload)
            .Must(payload => !string.IsNullOrWhiteSpace(payload))
            .WithMessage("'Payload' must not be empty.");

        RuleFor(x => x.StripeSignature)
            .Must(signature => !string.IsNullOrWhiteSpace(signature))
            .WithMessage("'StripeSignature' must not be empty.");
    }
}
