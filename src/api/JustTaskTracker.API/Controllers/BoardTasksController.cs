using JustTaskTracker.API.Extensions;
using JustTaskTracker.Application.Boards.Commands.Attachments;
using JustTaskTracker.Application.Boards.Commands.BoardTasks;
using JustTaskTracker.Application.Boards.Commands.Comments;
using JustTaskTracker.Application.Boards.Queries.BoardTasks;
using JustTaskTracker.Application.Boards.Queries.Comments;
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
    [HttpGet]
    public async Task<IActionResult> GetLookupList(Guid boardId, [FromQuery] GetBoardTasksLookupQuery request, CancellationToken ct)
    {
        var result = await sender.Send(request with { BoardId = boardId }, ct);

        return result.Match(
            data => Ok(data),
            error => error.CreateErrorResponse());
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new GetBoardTaskByIdQuery(id), ct);

        return result.Match(
            data => Ok(data),
            error => error.CreateErrorResponse());
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid columnId, [FromBody] CreateBoardTaskCommand request, CancellationToken ct)
    {
        var result = await sender.Send(request with { ColumnId = columnId }, ct);

        return result.Match(
            data => Ok(data),
            error => error.CreateErrorResponse());
    }

    [HttpPut("{id:guid}/position")]
    public async Task<IActionResult> Reorder(Guid columnId, Guid id, [FromBody] ReorderBoardTaskCommand request, CancellationToken ct)
    {
        var result = await sender.Send(request with { TargetColumnId = columnId, BoardTaskId = id }, ct);

        return result.Match(
            () => NoContent(),
            error => error.CreateErrorResponse());
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Update(Guid boardId, Guid id, [FromBody] UpdateBoardTaskCommand request, CancellationToken ct)
    {
        var result = await sender.Send(request with { BoardId = boardId, BoardTaskId = id }, ct);

        return result.Match(
            () => NoContent(),
            error => error.CreateErrorResponse());
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid columnId, Guid id, CancellationToken ct)
    {
        var result = await sender.Send(new DeleteBoardTaskCommand(columnId, id), ct);

        return result.Match(
            () => NoContent(),
            error => error.CreateErrorResponse());
    }

    [HttpGet("{id:guid}/comments")]
    public async Task<IActionResult> GetComments(Guid id, [FromQuery] GetBoardTaskCommentsQuery request, CancellationToken ct)
    {
        var result = await sender.Send(
            request with { BoardTaskId = id },
            ct);

        return result.Match(
            data => Ok(data),
            error => error.CreateErrorResponse());
    }

    [HttpPost("{id:guid}/comments")]
    public async Task<IActionResult> CreateComment(Guid id, [FromBody] CreateBoardTaskCommentCommand request, CancellationToken ct)
    {
        var result = await sender.Send(
            request with { BoardTaskId = id },
            ct);

        return result.Match(
            data => Ok(data),
            error => error.CreateErrorResponse());
    }

    [HttpPatch("{id:guid}/comments/{commentId:guid}")]
    public async Task<IActionResult> UpdateComment(Guid commentId, [FromBody] UpdateBoardTaskCommentCommand request, CancellationToken ct)
    {
        var result = await sender.Send(
            request with { CommentId = commentId },
            ct);

        return result.Match(
            () => NoContent(),
            error => error.CreateErrorResponse());
    }

    [HttpDelete("{id:guid}/comments/{commentId:guid}")]
    public async Task<IActionResult> DeleteComment(Guid commentId, CancellationToken ct)
    {
        var result = await sender.Send(
            new DeleteBoardTaskCommentCommand(commentId),
            ct);

        return result.Match(
            () => NoContent(),
            error => error.CreateErrorResponse());
    }

    [HttpPost("{id:guid}/attachments")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadAttachment(Guid id, IFormFile file, CancellationToken ct)
    {
        var result = await sender.Send(new UploadBoardTaskAttachmentCommand(id, file), ct);

        return result.Match(
            data => Ok(data),
            error => error.CreateErrorResponse());
    }

    [HttpGet("{id:guid}/attachments/{attachmentId:guid}")]
    public async Task<IActionResult> DownloadAttachment(Guid attachmentId, CancellationToken ct)
    {
        var result = await sender.Send(
            new DownloadBoardTaskAttachmentCommand(attachmentId),
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
    public async Task<IActionResult> DeleteAttachment(Guid id, Guid attachmentId, CancellationToken ct)
    {
        var result = await sender.Send(
            new DeleteBoardTaskAttachmentCommand(id, attachmentId),
            ct);

        return result.Match(
            () => NoContent(),
            error => error.CreateErrorResponse());
    }
}