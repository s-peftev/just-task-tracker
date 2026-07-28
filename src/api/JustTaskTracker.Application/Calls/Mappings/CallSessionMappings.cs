using JustTaskTracker.Application.Calls.ReadModels;
using JustTaskTracker.Application.Users.Mappings;
using JustTaskTracker.Application.Users.ReadModels;
using JustTaskTracker.Domain.Calls.DTOs;
using JustTaskTracker.Domain.Calls.Enums;

namespace JustTaskTracker.Application.Calls.Mappings;

public static class CallSessionMappings
{
    public static CallSessionDto ToDto(this CallSessionWithStateReadModel session, Func<UserReadModel, string?> profilePhotoUrlResolver) =>
        new(
            session.Id,
            session.BoardId,
            session.CreatedBy.ToDto(profilePhotoUrlResolver),
            session.Title,
            session.Topic,
            session.Visibility,
            session.AcsRoomId,
            session.Status,
            session.StartedAtUtc,
            // Client-side "can I join?" gate for Restricted sessions (AD-4) -- Open sessions don't need it.
            session.Visibility == CallVisibility.Restricted
                ? session.AllowedUsers.Select(u => u.ToDto(profilePhotoUrlResolver)).ToList()
                : null,
            session.LinkedTasks,
            session.Participants.Select(u => u.ToDto(profilePhotoUrlResolver)).ToList(),
            session.CurrentPresenterUserId);
}
