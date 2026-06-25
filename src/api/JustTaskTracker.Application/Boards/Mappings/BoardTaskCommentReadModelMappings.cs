using JustTaskTracker.Application.Boards.ReadModels;
using JustTaskTracker.Application.Users.Mappings;
using JustTaskTracker.Application.Users.ReadModels;
using JustTaskTracker.Domain.Boards.DTOs.Comments;

namespace JustTaskTracker.Application.Boards.Mappings;

public static class BoardTaskCommentReadModelMappings
{
    public static BoardTaskCommentDto ToDto(
        this BoardTaskCommentReadModel comment,
        Func<UserReadModel, string?> profilePhotoUrlResolver) =>
        new(
            comment.Id,
            comment.Body,
            comment.CreatedAtUtc,
            comment.Author.ToDto(profilePhotoUrlResolver),
            comment.LastModifiedAtUtc);
}
