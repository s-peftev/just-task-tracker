using JustTaskTracker.Application.Calls.ReadModels;
using JustTaskTracker.Domain.Auth.DTOs;
using JustTaskTracker.Domain.Boards.DTOs.BoardTasks;
using JustTaskTracker.Domain.Calls.DTOs;
using JustTaskTracker.Domain.Calls.Entities;

namespace JustTaskTracker.Application.Calls.Mappings;

public static class CallSessionHistoryMappings
{
    public static CallSessionHistoryDto ToHistoryDto(
        this CallSession session,
        UserDto createdBy,
        IReadOnlyList<BoardTaskLookupDto> linkedTasks,
        IReadOnlyList<CallHistoryParticipantDto> participants,
        IReadOnlyList<UserDto>? allowedUsers) =>
        new(
            session.Id,
            session.Title,
            session.Topic,
            session.Visibility,
            createdBy,
            session.StartedAtUtc,
            session.EndedAtUtc!.Value,
            linkedTasks,
            participants,
            allowedUsers);

    public static CallHistoryParticipantDto ToDto(this CallParticipantHistoryReadModel entry, UserDto user) =>
        new(user, entry.FirstJoinedAtUtc, entry.LastLeftAtUtc);
}
