namespace JustTaskTracker.Application.Calls.Abstractions;

public record AcsCallToken(string Token, DateTimeOffset ExpiresOn);

public interface IAcsCallProvisioningService
{
    Task<string> CreateRoomAsync(CancellationToken ct = default);

    Task DeleteRoomAsync(string acsRoomId, CancellationToken ct = default);

    Task<AcsCallToken> IssueJoinTokenAsync(Guid userId, string acsRoomId, CancellationToken ct = default);

    /// <summary>
    /// Removes the given users from the Room's participant list, which disconnects any of them
    /// currently in the call (AD-15) -- the trigger for a force-end, not a direct state write.
    /// Users with no known ACS identity mapping are skipped (they were never in the Room).
    /// </summary>
    Task RemoveParticipantsAsync(string acsRoomId, IReadOnlyList<Guid> userIds, CancellationToken ct = default);
}
