using JustTaskTracker.API.Extensions;
using JustTaskTracker.Application.Assistant.Commands;
using JustTaskTracker.Application.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JustTaskTracker.API.Controllers;

[Route("assistant")]
[ApiController]
[Authorize(Policy = AuthorizationPolicies.IsAppMember)]
public class AssistantController(ISender sender) : ControllerBase
{
    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] AskAssistantCommand command, CancellationToken ct)
    {
        var result = await sender.Send(command, ct);

        return result.Match(
            data => Ok(data),
            error => error.CreateErrorResponse());
    }
}
