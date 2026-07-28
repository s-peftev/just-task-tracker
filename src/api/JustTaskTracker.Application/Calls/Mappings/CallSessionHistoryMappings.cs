using JustTaskTracker.Application.Calls.ReadModels;
using JustTaskTracker.Application.Users.Mappings;
using JustTaskTracker.Application.Users.ReadModels;
using JustTaskTracker.Domain.Auth.DTOs;
using JustTaskTracker.Domain.Calls.DTOs;
using JustTaskTracker.Domain.Calls.Enums;

namespace JustTaskTracker.Application.Calls.Mappings;

public static class CallSessionHistoryMappings
{
    public static CallSessionHistoryDto ToDto(this CallSessionHistoryReadModel session, Func<UserReadModel, string?> profilePhotoUrlResolver)
    {
        var participants = session.ParticipantEvents
            .GroupBy(e => e.User.Id)
            .Select(g => new CallHistoryParticipantDto(
                g.First().User.ToDto(profilePhotoUrlResolver),
                g.Min(e => e.JoinedAtUtc),
                g.Max(e => (DateTime?)e.LeftAtUtc)))
            .ToList();

        IReadOnlyList<UserDto>? allowedUsers = session.Visibility == CallVisibility.Restricted
            ? session.AllowedUsers.Select(u => u.ToDto(profilePhotoUrlResolver)).ToList()
            : null;

        return new CallSessionHistoryDto(
            session.Id,
            session.Title,
            session.Topic,
            session.Visibility,
            session.CreatedBy.ToDto(profilePhotoUrlResolver),
            session.StartedAtUtc,
            session.EndedAtUtc,
            session.LinkedTasks,
            participants,
            allowedUsers);
    }
}
