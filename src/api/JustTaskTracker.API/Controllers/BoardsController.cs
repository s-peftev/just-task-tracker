using JustTaskTracker.API.Extensions;
using JustTaskTracker.Application.Common.Constants;
using JustTaskTracker.Application.Kanban.Commands;
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
        var result = await sender.Send(new GetBoardByIdQuery(id), ct);

        return result.Match(
            data => Ok(data),
            error => error.CreateErrorResponse());
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.IsAppContributor)]
    public async Task<IActionResult> Create([FromBody] CreateBoardCommand request, CancellationToken ct)
    {
        var result = await sender.Send(request, ct);

        return result.Match(
            data => Ok(data),
            error => error.CreateErrorResponse());
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.IsAppContributor)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBoardCommand request, CancellationToken ct)
    {
        var result = await sender.Send(request with { BoardId = id }, ct);

        return result.Match(
            data => Ok(data),
            error => error.CreateErrorResponse());
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AuthorizationPolicies.IsAppContributor)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new DeleteBoardCommand(id), ct);

        return result.Match(
            () => NoContent(),
            error => error.CreateErrorResponse());
    }
}
