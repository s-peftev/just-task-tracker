using JustTaskTracker.Application.Boards.ReadModels;
using JustTaskTracker.Application.Users.Mappings;
using JustTaskTracker.Application.Users.ProfilePhotos;
using JustTaskTracker.Domain.Boards.DTOs.Comments;

namespace JustTaskTracker.Application.Boards.Mappings;

public static class BoardTaskCommentReadModelMappings
{
    public static BoardTaskCommentDto ToDto(
        this BoardTaskCommentReadModel comment,
        IProfilePhotoService profilePhotoService) =>
        new(
            comment.Id,
            comment.Body,
            comment.CreatedAtUtc,
            comment.Author.ToDto(profilePhotoService),
            comment.LastModifiedAtUtc);
}
