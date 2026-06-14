using JustTaskTracker.API.Extensions;
using JustTaskTracker.Application.Boards.Commands;
using JustTaskTracker.Application.Boards.Queries;
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
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid boardId, Guid columnId, Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetBoardTaskByIdQuery(boardId, columnId, id), ct);

        return result.Match(
            data => Ok(data),
            error => error.CreateErrorResponse());
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid boardId, Guid columnId, [FromBody] CreateBoardTaskCommand request, CancellationToken ct)
    {
        var result = await sender.Send(request with { BoardId = boardId, ColumnId = columnId }, ct);

        return result.Match(
            data => Ok(data),
            error => error.CreateErrorResponse());
    }

    [HttpPut("{id:guid}/position")]
    public async Task<IActionResult> Reorder(Guid boardId, Guid columnId, Guid id, [FromBody] ReorderBoardTaskCommand request, CancellationToken ct)
    {
        var result = await sender.Send(request with { BoardId = boardId, TargetColumnId = columnId, BoardTaskId = id }, ct);

        return result.Match(
            () => NoContent(),
            error => error.CreateErrorResponse());
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid boardId, Guid columnId, Guid id, [FromBody] UpdateBoardTaskCommand request, CancellationToken ct)
    {
        var result = await sender.Send(request with { BoardId = boardId, ColumnId = columnId, BoardTaskId = id }, ct);

        return result.Match(
            () => NoContent(),
            error => error.CreateErrorResponse());
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid boardId, Guid columnId, Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new DeleteBoardTaskCommand(boardId, columnId, id), ct);

        return result.Match(
            () => NoContent(),
            error => error.CreateErrorResponse());
    }

    [HttpGet("{id:guid}/comments")]
    public async Task<IActionResult> GetComments(
        Guid boardId,
        Guid columnId,
        Guid id,
        [FromQuery] GetBoardTaskCommentsQuery request,
        CancellationToken ct)
    {
        var result = await sender.Send(
            request with { BoardId = boardId, ColumnId = columnId, BoardTaskId = id },
            ct);

        return result.Match(
            data => Ok(data),
            error => error.CreateErrorResponse());
    }

    [HttpPost("{id:guid}/attachments")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadAttachment(Guid boardId, Guid columnId, Guid id, IFormFile file, CancellationToken ct)
    {
        var result = await sender.Send(new UploadBoardTaskAttachmentCommand(boardId, columnId, id, file), ct);

        return result.Match(
            data => Ok(data),
            error => error.CreateErrorResponse());
    }

    [HttpGet("{id:guid}/attachments/{attachmentId:guid}")]
    public async Task<IActionResult> DownloadAttachment(Guid boardId, Guid columnId, Guid id, Guid attachmentId, CancellationToken ct)
    {
        var result = await sender.Send(
            new DownloadBoardTaskAttachmentCommand(boardId, columnId, id, attachmentId),
            ct);

        return result.Match(
            download => File(
                download.Blob.Content,
                download.Blob.ContentType,
                download.OriginalFileName,
                enableRangeProcessing: true),
            error => error.CreateErrorResponse());
    }

    [HttpDelete("{id:guid}/attachments/{attachmentId:guid}")]
    public async Task<IActionResult> DeleteAttachment(Guid boardId, Guid columnId, Guid id, Guid attachmentId, CancellationToken ct)
    {
        var result = await sender.Send(
            new DeleteBoardTaskAttachmentCommand(boardId, columnId, id, attachmentId),
            ct);

        return result.Match(
            () => NoContent(),
            error => error.CreateErrorResponse());
    }
}