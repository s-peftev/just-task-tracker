using JustTaskTracker.Infrastructure.Auth.Constants;
using Microsoft.AspNetCore.SignalR;

namespace JustTaskTracker.Infrastructure.Calls.Auth;

// AD-10: the default IUserIdProvider reads ClaimTypes.NameIdentifier, which this app never
// issues -- AuthenticationModule disables default claim mapping and uses "oid" as the identity
// claim instead. Without this, Clients.User(...) would never match any connection.
public class AzureAdObjectIdUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection) =>
        connection.User?.FindFirst(EntraClaimTypes.ObjectId)?.Value;
}
