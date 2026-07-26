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

    public async Task<AcsCallToken> IssueJoinTokenAsync(Guid userId, string acsRoomId, CancellationToken ct = default)
    {
        var identifier = await ResolveAcsIdentityAsync(userId, ct);

        await roomsClient.AddOrUpdateParticipantsAsync(acsRoomId, [new RoomParticipant(identifier)], ct);

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
