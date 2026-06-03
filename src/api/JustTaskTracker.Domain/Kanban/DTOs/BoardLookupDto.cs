using JustTaskTracker.Domain.Auth.DTOs;
using JustTaskTracker.Domain.Kanban.Enums;

namespace JustTaskTracker.Domain.Kanban.DTOs;

public record BoardLookupDto(
    Guid Id,
    string Name,
    DateTime CreatedAtUtc,
    UserDto? Owner,
    BoardMemberRole UserRole);
