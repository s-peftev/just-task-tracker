using JustTaskTracker.Domain.Boards.Enums;

namespace JustTaskTracker.Application.Boards.ReadModels;

public record BoardMemberIdentity(Guid UserId, Guid AzureAdObjectId, BoardMemberRole Role);
