using JustTaskTracker.Domain.Boards.Enums;

namespace JustTaskTracker.Application.Boards.ReadModels;

public record BoardLookupReadModel(
    Guid Id,
    string Name,
    bool IsArchived,
    BoardMemberRole UserRole,
    string OwnerEmail,
    string? OwnerDisplayName,
    DateTime? ArchivedAtUtc);
