using JustTaskTracker.API.Extensions;
using JustTaskTracker.Application.Boards.Commands;
using JustTaskTracker.Application.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JustTaskTracker.API.Controllers;

[Route("boards/{boardId:guid}/columns")]
[ApiController]
[Authorize(Policy = AuthorizationPolicies.IsAppMember)]
public class ColumnsController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(Guid boardId, [FromBody] CreateColumnCommand request, CancellationToken ct)
    {
        var result = await sender.Send(request with { BoardId = boardId }, ct);

        return result.Match(
            data => Ok(data),
            error => error.CreateErrorResponse());
    }

    [HttpPut("{id:guid}/position")]
    public async Task<IActionResult> Reorder(Guid boardId, Guid id, [FromBody] ReorderColumnsCommand request, CancellationToken ct)
    {
        var result = await sender.Send(request with { BoardId = boardId, ColumnId = id }, ct);

        return result.Match(
            () => NoContent(),
            error => error.CreateErrorResponse());
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid boardId, Guid id, [FromBody] UpdateColumnCommand request, CancellationToken ct)
    {
        var result = await sender.Send(request with { BoardId = boardId, ColumnId = id }, ct);

        return result.Match(
            () => NoContent(),
            error => error.CreateErrorResponse());
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid boardId, Guid id, [FromBody] DeleteColumnCommand request, CancellationToken ct)
    {
        var result = await sender.Send(request with { BoardId = boardId, ColumnId = id }, ct);

        return result.Match(
            () => NoContent(),
            error => error.CreateErrorResponse());
    }
}
