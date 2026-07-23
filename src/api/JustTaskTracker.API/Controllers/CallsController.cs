using JustTaskTracker.API.Extensions;
using JustTaskTracker.Application.Calls.Commands;
using JustTaskTracker.Application.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JustTaskTracker.API.Controllers;

[Route("calls")]
[ApiController]
public class CallsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.IsAppMember)]
    public async Task<IActionResult> Create([FromBody] CreateCallCommand request, CancellationToken ct)
    {
        var result = await sender.Send(request, ct);

        return result.Match(
            data => Ok(data),
            error => error.CreateErrorResponse());
    }

    [HttpPost("{id:guid}/join")]
    [Authorize(Policy = AuthorizationPolicies.IsAppMember)]
    public async Task<IActionResult> Join(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new JoinCallCommand(id), ct);

        return result.Match(
            data => Ok(data),
            error => error.CreateErrorResponse());
    }
}
