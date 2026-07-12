using JustTaskTracker.API.Extensions;
using JustTaskTracker.Application.Billing.Queries;
using JustTaskTracker.Application.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JustTaskTracker.API.Controllers;

[Route("billing")]
[ApiController]
[Authorize(Policy = AuthorizationPolicies.IsAppMember)]
public class BillingController(ISender sender) : ControllerBase
{
    [HttpGet("entitlements")]
    public async Task<IActionResult> GetEntitlements(CancellationToken ct)
    {
        var result = await sender.Send(new GetCurrentEntitlementsUserQuery(), ct);

        return result.Match(
            data => Ok(data),
            error => error.CreateErrorResponse());
    }
}
