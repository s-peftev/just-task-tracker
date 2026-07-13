using System.Text;
using JustTaskTracker.API.Filters;
using JustTaskTracker.Application.Billing.Commands;
using JustTaskTracker.Domain.Common.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JustTaskTracker.API.Controllers;

[Route("webhooks")]
[ApiController]
[AllowAnonymous]
[SkipApiResponseEnvelope]
public class WebhookController(ISender sender) : ControllerBase
{
    [HttpPost("stripe")]
    public async Task<IActionResult> HandleStripeWebhook(CancellationToken ct)
    {
        using var reader = new StreamReader(Request.Body, Encoding.UTF8);
        var payload = await reader.ReadToEndAsync(ct);
        var signature = Request.Headers["Stripe-Signature"].ToString();

        var result = await sender.Send(new HandleBillingWebhookCommand(payload, signature), ct);

        return result.Match<IActionResult>(
            () => Ok(),
            error => error.Type is ErrorType.ServiceUnavailable or ErrorType.InternalServerError
                ? StatusCode(StatusCodes.Status503ServiceUnavailable)
                : BadRequest());
    }
}
