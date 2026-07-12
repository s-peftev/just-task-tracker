using JustTaskTracker.API.Extensions;
using JustTaskTracker.Application.Billing.Queries;
using JustTaskTracker.Application.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JustTaskTracker.API.Controllers;

[Route("billing")]
[ApiController]
public class BillingController(ISender sender) : ControllerBase
{
    [HttpGet("entitlements")]
    [Authorize(Policy = AuthorizationPolicies.IsAppMember)]
    public async Task<IActionResult> GetEntitlements(CancellationToken ct)
    {
        var result = await sender.Send(new GetCurrentUserEntitlementsQuery(), ct);

        return result.Match(
            data => Ok(data),
            error => error.CreateErrorResponse());
    }

    [HttpGet("subscription")]
    [Authorize(Policy = AuthorizationPolicies.IsAppUser)]
    public async Task<IActionResult> GetUserSubscription(CancellationToken ct)
    {
        var result = await sender.Send(new GetCurrentUserSubscriptionQuery(), ct);

        return result.Match(
            data => Ok(data),
            error => error.CreateErrorResponse());
    }
}
