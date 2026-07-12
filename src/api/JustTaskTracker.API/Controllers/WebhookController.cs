using System.Text;
using JustTaskTracker.API.Filters;
using JustTaskTracker.Application.Billing.Abstractions;
using JustTaskTracker.Application.Billing.Commands;
using JustTaskTracker.Domain.Common.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace JustTaskTracker.API.Controllers;

[Route("webhooks")]
[ApiController]
[AllowAnonymous]
[SkipApiResponseEnvelope]
public class WebhookController(
    ISender sender,
    IBillingService billingService,
    ILogger<WebhookController> logger) : ControllerBase
{
    [HttpPost("stripe")]
    public async Task<IActionResult> HandleStripeWebhook(CancellationToken ct)
    {
        var signature = Request.Headers["Stripe-Signature"].ToString();

        if (string.IsNullOrWhiteSpace(signature))
            return BadRequest();

        string payload;
        using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
            payload = await reader.ReadToEndAsync(ct);

        if (string.IsNullOrWhiteSpace(payload))
            return BadRequest();

        try
        {
            var parsedEvent = await billingService.ParseWebhookEventAsync(payload, signature, ct);
            var result = await sender.Send(new HandleBillingWebhookCommand(parsedEvent), ct);

            return result.Match<IActionResult>(
                () => Ok(),
                error => error.Type is ErrorType.ServiceUnavailable or ErrorType.InternalServerError
                    ? StatusCode(StatusCodes.Status503ServiceUnavailable)
                    : BadRequest());
        }
        catch (StripeException ex)
        {
            logger.LogWarning(ex, "Rejected Stripe webhook due to invalid signature or payload.");
            return BadRequest();
        }
    }
}
