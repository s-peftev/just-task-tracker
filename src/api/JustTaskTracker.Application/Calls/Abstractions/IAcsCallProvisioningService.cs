namespace JustTaskTracker.Application.Calls.Abstractions;

public record AcsCallToken(string Token, DateTimeOffset ExpiresOn);

public interface IAcsCallProvisioningService
{
    Task<string> CreateRoomAsync(CancellationToken ct = default);

    Task DeleteRoomAsync(string acsRoomId, CancellationToken ct = default);

    Task<AcsCallToken> IssueJoinTokenAsync(Guid userId, string acsRoomId, CancellationToken ct = default);
}
