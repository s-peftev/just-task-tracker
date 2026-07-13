using JustTaskTracker.API.Extensions;
using JustTaskTracker.Application.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using JustTaskTracker.Application.Boards.Commands.Boards;
using JustTaskTracker.Application.Boards.Queries.Boards;
using JustTaskTracker.Domain.Boards.DTOs.Boards;
using JustTaskTracker.Domain.Billing.Constants;

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
            () => NoContent(),
            error => error.CreateErrorResponse());
    }

    [HttpPost("{id:guid}/archive/export")]
    [Authorize(Policy = AuthorizationPolicies.IsAppMember)]
    public async Task<IActionResult> ArchiveAndExport(Guid id, [FromBody] BoardExportOptions exportOptions, CancellationToken ct)
    {
        var result = await sender.Send(new ArchiveAndExportBoardCommand(id, exportOptions, Features.BoardExport), ct);

        return result.Match(
            data => Ok(data),
            error => error.CreateErrorResponse());
    }

    [HttpGet("{id:guid}/archive/export")]
    [Authorize(Policy = AuthorizationPolicies.IsAppMember)]
    public async Task<IActionResult> GetArchiveExport(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetBoardArchiveQuery(id, Features.BoardArchiveDownload), ct);

        return result.Match(
            data => Ok(data),
            error => error.CreateErrorResponse());
    }

    [HttpPost("{id:guid}/archive/re-export")]
    [Authorize(Policy = AuthorizationPolicies.IsAppMember)]
    public async Task<IActionResult> ReExportArchived(Guid id, [FromBody] BoardExportOptions reExportOptions, CancellationToken ct)
    {
        var result = await sender.Send(new ReExportArchivedBoardCommand(id, reExportOptions, Features.BoardReExport), ct);

        return result.Match(
            () => NoContent(),
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

    [HttpGet("{id:guid}/members")]
    [Authorize(Policy = AuthorizationPolicies.IsAppMember)]
    public async Task<IActionResult> GetMembers(Guid id, [FromQuery] GetBoardMembersQuery request, CancellationToken ct)
    {
        var result = await sender.Send(request with { BoardId = id }, ct);

        return result.Match(
            data => Ok(data),
            error => error.CreateErrorResponse());
    }

    [HttpPost("{id:guid}/members")]
    [Authorize(Policy = AuthorizationPolicies.IsAppMember)]
    public async Task<IActionResult> AddMember(Guid id, [FromBody] AddBoardMemberCommand request, CancellationToken ct)
    {
        var result = await sender.Send(request with { BoardId = id }, ct);

        return result.Match(
            () => NoContent(),
            error => error.CreateErrorResponse());
    }

    [HttpPut("{id:guid}/members/{userId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.IsAppMember)]
    public async Task<IActionResult> UpdateMember(Guid id, Guid userId, [FromBody] UpdateBoardMemberCommand request, CancellationToken ct)
    {
        var result = await sender.Send(request with { BoardId = id, UserId = userId }, ct);

        return result.Match(
            () => NoContent(),
            error => error.CreateErrorResponse());
    }

    [HttpDelete("{id:guid}/members/{userId:guid}")]
    [Authorize(Policy = AuthorizationPolicies.IsAppMember)]
    public async Task<IActionResult> DeleteMember(Guid id, Guid userId, CancellationToken ct)
    {
        var result = await sender.Send(new DeleteBoardMemberCommand(id, userId), ct);

        return result.Match(
            () => NoContent(),
            error => error.CreateErrorResponse());
    }

    [HttpPost("{id:guid}/leave")]
    [Authorize(Policy = AuthorizationPolicies.IsAppMember)]
    public async Task<IActionResult> Leave(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new LeaveBoardCommand(id), ct);

        return result.Match(
            () => NoContent(),
            error => error.CreateErrorResponse());
    }
}
