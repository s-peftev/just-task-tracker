using JustTaskTracker.Application.Boards.ReadModels;
using JustTaskTracker.Application.Users.Mappings;
using JustTaskTracker.Application.Users.ProfilePhotos;
using JustTaskTracker.Domain.Boards.DTOs.BoardTasks;
using JustTaskTracker.Domain.Boards.Enums;

namespace JustTaskTracker.Application.Boards.Mappings;

public static class BoardTaskDetailsReadModelMappings
{
    public static BoardTaskDetailsDto ToDto(
        this BoardTaskDetailsReadModel task,
        IProfilePhotoService profilePhotoService,
        BoardMemberRole userRole) =>
        new(
            task.Id,
            task.ColumnId,
            task.ColumnName,
            task.Title,
            task.Position,
            task.CreatedAtUtc,
            task.Reporter.ToDto(profilePhotoService),
            userRole,
            task.Attachments.Select(attachment => attachment.ToDto(profilePhotoService)).ToList(),
            task.Description,
            task.Assignee.ToNullableDto(profilePhotoService));
}
