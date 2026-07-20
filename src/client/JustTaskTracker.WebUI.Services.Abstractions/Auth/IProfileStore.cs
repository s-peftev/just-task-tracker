using JustTaskTracker.WebUI.Domain.Auth;

namespace JustTaskTracker.WebUI.Services.Abstractions.Auth;

public interface IProfileStore
{
    UserWithRolesDto? Profile { get; }
    bool IsLoading { get; }
    bool IsLoaded { get; }

    /// <summary>
    /// True when the API rejected the token (401/403). The component should
    /// redirect to /login so MSAL can acquire a fresh token.
    /// </summary>
    bool RequiresLogin { get; }

    string? ErrorMessage { get; }

    event Action? StateChanged;

    /// <summary>
    /// Ensures the profile is loaded via GET /auth/me. No-ops if already loaded;
    /// attaches to the in-flight load if one is running; starts a new load otherwise.
    /// Use for session restore (F5 / silent MSAL). If me returns 404, provisions once
    /// via POST /auth/login.
    /// </summary>
    Task EnsureLoadedAsync(CancellationToken ct = default);

    /// <summary>
    /// Forces a fresh profile load via GET /auth/me regardless of current state.
    /// Safe to call concurrently — subsequent callers attach to the same in-flight refresh.
    /// </summary>
    Task RefreshAsync(CancellationToken ct = default);

    /// <summary>
    /// Loads (or refreshes) the profile via POST /auth/login — provisions the app user
    /// and syncs global roles from the token. Use after interactive Entra login,
    /// account switch, or when the cached profile does not match the auth principal.
    /// </summary>
    Task LoginAsync(CancellationToken ct = default);

    /// <summary>
    /// Clears all cached profile state. Call on logout before MSAL redirect.
    /// </summary>
    void Reset();

    /// <summary>
    /// Updates the cached profile photo URL after a successful upload.
    /// </summary>
    void SetProfilePhotoUrl(string profilePhotoUrl);

    /// <summary>
    /// Clears the cached profile photo URL after a successful delete.
    /// </summary>
    void ClearProfilePhotoUrl();
}
