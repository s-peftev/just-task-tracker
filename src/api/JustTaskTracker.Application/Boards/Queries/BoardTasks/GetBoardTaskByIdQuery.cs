using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Common.Interfaces;
using JustTaskTracker.Application.Users.ProfilePhotos;
using JustTaskTracker.Domain.Auth.DTOs;
using JustTaskTracker.Domain.Boards.Authorization;
using JustTaskTracker.Domain.Boards.DTOs.Attachments;
using JustTaskTracker.Domain.Boards.DTOs.BoardTasks;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;

namespace JustTaskTracker.Application.Boards.Queries.BoardTasks;

public record GetBoardTaskByIdQuery(Guid BoardTaskId) : IRequest<Result<BoardTaskDetailsDto>>;

public class GetBoardTaskByIdQueryHandler(
    ICurrentUserAccessor currentUserAccessor,
    IBoardTaskRepository boardTaskRepository,
    IProfilePhotoService profilePhotoService)
    : IRequestHandler<GetBoardTaskByIdQuery, Result<BoardTaskDetailsDto>>
{
    public async Task<Result<BoardTaskDetailsDto>> Handle(GetBoardTaskByIdQuery request, CancellationToken ct)
    {
        var userRole = await boardTaskRepository.GetUserRoleAsync(request.BoardTaskId, currentUserAccessor.AzureAdObjectId, ct);

        if (userRole is not { } authorizedRole || !BoardRolePermissions.CanViewBoard(authorizedRole))
            return Result<BoardTaskDetailsDto>.Failure(GeneralErrors.Forbidden);

        var taskInfo = await boardTaskRepository.GetBoardTaskDetailsAsync(request.BoardTaskId, ct);

        if (taskInfo is null)
            return Result<BoardTaskDetailsDto>.Failure(GeneralErrors.NotFound);

        var task = new BoardTaskDetailsDto(
            taskInfo.Id,
            taskInfo.ColumnId,
            taskInfo.ColumnName,
            taskInfo.Title,
            taskInfo.Position,
            taskInfo.CreatedAtUtc,
            new UserDto(
                taskInfo.Reporter.Id,
                taskInfo.Reporter.Email,
                taskInfo.Reporter.DisplayName,
                taskInfo.Reporter.ProfilePhotoVersion is null ? null : profilePhotoService.BuildThumbnailUrl(taskInfo.Reporter.Id)),
            userRole!.Value,
            taskInfo.Attachments.Select(a => new BoardTaskAttachmentDto(
                a.Id,
                a.OriginalFileName,
                a.ContentType,
                a.FileSizeBytes,
                a.Position,
                a.CreatedAtUtc,
                new UserDto(
                    a.UploadedBy.Id,
                    a.UploadedBy.Email,
                    a.UploadedBy.DisplayName,
                    a.UploadedBy.ProfilePhotoVersion is null ? null : profilePhotoService.BuildThumbnailUrl(a.UploadedBy.Id)))).ToList(),
            taskInfo.Description,
            taskInfo.Assignee is null
                ? null
                : new UserDto(
                    taskInfo.Assignee.Id,
                    taskInfo.Assignee.Email,
                    taskInfo.Assignee.DisplayName,
                    taskInfo.Assignee.ProfilePhotoVersion is null ? null : profilePhotoService.BuildThumbnailUrl(taskInfo.Assignee.Id)));

        return Result<BoardTaskDetailsDto>.Success(task);
    }
}
