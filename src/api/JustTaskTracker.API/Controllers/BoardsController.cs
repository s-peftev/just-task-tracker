using JustTaskTracker.API.Extensions;
using JustTaskTracker.Application.Common.Constants;
using JustTaskTracker.Application.Kanban.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JustTaskTracker.API.Controllers;

[Route("boards")]
[ApiController]
public class BoardsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = AuthorizationPolicies.IsAppMember)]
    public async Task<IActionResult> GetMy([FromQuery] GetBoardsForCurrentUserQuery request, CancellationToken ct)
    { 
        var result = await sender.Send(request, ct);

        return result.Match(
            data => Ok(data),
            error => error.CreateErrorResponse());
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.IsAppMember)]
    public async Task<IActionResult> GetMyById(Guid id, CancellationToken ct)
    {
        return Ok();
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.IsAppContributor)]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        return Ok();
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.IsAppContributor)]
    public async Task<IActionResult> Update(Guid id, CancellationToken ct)
    {
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.IsAppContributor)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        return Ok();
    }
}
