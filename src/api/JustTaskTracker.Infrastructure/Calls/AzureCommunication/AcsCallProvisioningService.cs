using Azure.Communication;
using Azure.Communication.Identity;
using Azure.Communication.Rooms;
using JustTaskTracker.Application.Calls.Abstractions;
using JustTaskTracker.Application.Calls.Repositories;
using JustTaskTracker.Application.Common.Persistence;
using JustTaskTracker.Domain.Calls.Entities;

namespace JustTaskTracker.Infrastructure.Calls.AzureCommunication;

// AD-6: ACS Custom ID (preview-only) is not used -- identity mapping is our own table,
// resolved (or created on first use) here, then the joining user is added as a Room
// participant before a token is issued, since ACS Rooms gate call access by participant
// membership, not by token possession alone.
public class AcsCallProvisioningService(
    RoomsClient roomsClient,
    CommunicationIdentityClient identityClient,
    IAcsUserIdentityMappingRepository mappingRepository,
    IUnitOfWork unitOfWork)
    : IAcsCallProvisioningService
{
    private static readonly TimeSpan TokenValidity = TimeSpan.FromMinutes(1440);

    public async Task<string> CreateRoomAsync(CancellationToken ct = default)
    {
        var room = await roomsClient.CreateRoomAsync(null, null, [], ct);

        return room.Value.Id;
    }

    public async Task DeleteRoomAsync(string acsRoomId, CancellationToken ct = default) =>
        await roomsClient.DeleteRoomAsync(acsRoomId, ct);

    public async Task RemoveParticipantsAsync(string acsRoomId, IReadOnlyList<Guid> userIds, CancellationToken ct = default)
    {
        var identifiers = new List<CommunicationIdentifier>();

        foreach (var userId in userIds)
        {
            var mapping = await mappingRepository.GetByUserIdAsync(userId, ct);

            if (mapping is not null)
                identifiers.Add(new CommunicationUserIdentifier(mapping.AcsCommunicationUserId));
        }

        if (identifiers.Count == 0)
            return;

        await roomsClient.RemoveParticipantsAsync(acsRoomId, identifiers, ct);
    }

    public async Task<AcsCallToken> IssueJoinTokenAsync(Guid userId, string acsRoomId, CancellationToken ct = default)
    {
        var identifier = await ResolveAcsIdentityAsync(userId, ct);

        // ACS Rooms gate capabilities by native participant role, independently of our own
        // AD-9 presenter lock -- the default role (Attendee) cannot negotiate a screen-share
        // media stream at all (ACS itself rejects it, separately from who our app currently
        // allows to hold the lock), so every joiner needs Presenter here for screen share
        // (Story 2.1) to be possible for anyone.
        var participant = new RoomParticipant(identifier) { Role = ParticipantRole.Presenter };

        await roomsClient.AddOrUpdateParticipantsAsync(acsRoomId, [participant], ct);

        var tokenResponse = await identityClient.GetTokenAsync(identifier, [CommunicationTokenScope.VoIP], TokenValidity, ct);

        return new AcsCallToken(tokenResponse.Value.Token, tokenResponse.Value.ExpiresOn);
    }

    private async Task<CommunicationUserIdentifier> ResolveAcsIdentityAsync(Guid userId, CancellationToken ct)
    {
        var mapping = await mappingRepository.GetByUserIdAsync(userId, ct);

        if (mapping is not null)
            return new CommunicationUserIdentifier(mapping.AcsCommunicationUserId);

        var identity = await identityClient.CreateUserAsync(ct);

        mappingRepository.Add(new AcsUserIdentityMapping
        {
            UserId = userId,
            AcsCommunicationUserId = identity.Value.Id
        });

        await unitOfWork.SaveChangesAsync(ct);

        return identity.Value;
    }
}
