using JustTaskTracker.API.Extensions;
using JustTaskTracker.Application.Boards.Commands;
using JustTaskTracker.Application.Common.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JustTaskTracker.API.Controllers;

[Route("boards/{boardId:guid}/columns/{columnId:guid}/tasks")]
[ApiController]
[Authorize(Policy = AuthorizationPolicies.IsAppMember)]
public class BoardTasksController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(Guid boardId, Guid columnId, [FromBody] CreateBoardTaskCommand request, CancellationToken ct)
    {
        var result = await sender.Send(request with { BoardId = boardId, ColumnId = columnId }, ct);

        return result.Match(
            data => Ok(data),
            error => error.CreateErrorResponse());
    }
}
