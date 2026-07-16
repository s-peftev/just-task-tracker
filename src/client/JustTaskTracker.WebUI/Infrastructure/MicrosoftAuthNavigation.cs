using System.Security.Claims;
using JustTaskTracker.WebUI.Domain.Auth;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.JSInterop;

namespace JustTaskTracker.WebUI.Infrastructure;

public enum AccountSwitchResult
{
    NotPending,
    Switched,
    SameAccountRequiresFullLogout
}

public static class MicrosoftAuthNavigation
{
    public const string SelectAccountPrompt = "select_account";
    public const string AutoSignInQueryParam = "autoSignIn";
    public const string LogoutReturnUrlStorageKey = "jtt.logoutReturnUrl";
    public const string SwitchAccountPendingStorageKey = "jtt.switchAccountPending";
    public const string SwitchAccountPreviousUserStorageKey = "jtt.switchAccountPreviousUser";

    /// <summary>
    /// Set on interactive Entra login success; consumed by MainLayout to call POST /auth/login
    /// (role sync) instead of GET /auth/me (session restore).
    /// </summary>
    public const string PendingAppLoginStorageKey = "jtt.pendingAppLogin";

    private const string LocalSignOutJsMethod = "jttAuth.localSignOut";

    public static void NavigateToMicrosoftLogin(
        NavigationManager navigation,
        string returnUrl = "boards")
    {
        var requestOptions = new InteractiveRequestOptions
        {
            Interaction = InteractionType.SignIn,
            ReturnUrl = returnUrl
        };

        requestOptions.TryAddAdditionalParameter("prompt", SelectAccountPrompt);

        navigation.NavigateToLogin("authentication/login", requestOptions);
    }

    public static async Task LocalSignOutAsync(
        IJSRuntime js,
        NavigationManager navigation,
        string returnUrl = "/login")
    {
        await js.InvokeVoidAsync(LocalSignOutJsMethod);
        navigation.NavigateTo(returnUrl, forceLoad: true);
    }

    public static async Task NavigateToFullLogoutAsync(
        IJSRuntime js,
        NavigationManager navigation,
        string returnUrl = "/")
    {
        await js.InvokeVoidAsync("sessionStorage.setItem", LogoutReturnUrlStorageKey, returnUrl);
        navigation.NavigateToLogout("authentication/logout", returnUrl);
    }

    public static async Task SwitchMicrosoftAccountAsync(
        IJSRuntime js,
        NavigationManager navigation,
        AuthenticationStateProvider authStateProvider)
    {
        var authState = await authStateProvider.GetAuthenticationStateAsync();
        var previousUser = GetAccountIdentifier(authState.User);

        await js.InvokeVoidAsync("sessionStorage.setItem", SwitchAccountPreviousUserStorageKey, previousUser);
        await js.InvokeVoidAsync("sessionStorage.setItem", SwitchAccountPendingStorageKey, "1");

        await js.InvokeVoidAsync(LocalSignOutJsMethod);

        NavigateToMicrosoftLogin(navigation);
    }

    public static async Task<AccountSwitchResult> TryCompleteAccountSwitchAsync(
        IJSRuntime js,
        AuthenticationStateProvider authStateProvider)
    {
        var pending = await js.InvokeAsync<string?>("sessionStorage.getItem", SwitchAccountPendingStorageKey);
        if (pending != "1")
            return AccountSwitchResult.NotPending;

        await js.InvokeVoidAsync("sessionStorage.removeItem", SwitchAccountPendingStorageKey);

        var previousUser = await js.InvokeAsync<string?>(
            "sessionStorage.getItem",
            SwitchAccountPreviousUserStorageKey);
        await js.InvokeVoidAsync("sessionStorage.removeItem", SwitchAccountPreviousUserStorageKey);

        if (string.IsNullOrWhiteSpace(previousUser))
            return AccountSwitchResult.Switched;

        var authState = await authStateProvider.GetAuthenticationStateAsync();
        var currentUser = GetAccountIdentifier(authState.User);

        return string.Equals(previousUser, currentUser, StringComparison.OrdinalIgnoreCase)
            ? AccountSwitchResult.SameAccountRequiresFullLogout
            : AccountSwitchResult.Switched;
    }

    /// <summary>
    /// Reads and clears <see cref="PendingAppLoginStorageKey"/>. Returns true when an
    /// interactive Entra login just completed and the app should POST /auth/login.
    /// </summary>
    public static async Task<bool> ConsumePendingAppLoginAsync(IJSRuntime js)
    {
        var pending = await js.InvokeAsync<string?>("sessionStorage.getItem", PendingAppLoginStorageKey);
        await js.InvokeVoidAsync("sessionStorage.removeItem", PendingAppLoginStorageKey);

        return pending == "1";
    }

    public static string GetAccountIdentifier(ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated != true)
            return string.Empty;

        foreach (var claimType in new[]
        {
            "oid",
            "http://schemas.microsoft.com/identity/claims/objectidentifier",
            ClaimTypes.NameIdentifier,
            "sub",
            "preferred_username",
            ClaimTypes.Email,
            ClaimTypes.Upn
        })
        {
            var value = user.FindFirst(claimType)?.Value;
            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }

        return user.Identity?.Name ?? string.Empty;
    }

    /// <summary>
    /// Returns false when cached profile email does not match any email-like claim on the token.
    /// Used to detect stale profile data after an MS account switch.
    /// </summary>
    public static bool ProfileMatchesAuth(UserWithRolesDto profile, ClaimsPrincipal user)
    {
        foreach (var claimType in new[]
        {
            "preferred_username",
            ClaimTypes.Email,
            ClaimTypes.Upn,
            "email",
            "unique_name"
        })
        {
            var value = user.FindFirst(claimType)?.Value;
            if (!string.IsNullOrWhiteSpace(value) &&
                value.Equals(profile.Email, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        var identifier = GetAccountIdentifier(user);
        return identifier.Contains('@') &&
               identifier.Equals(profile.Email, StringComparison.OrdinalIgnoreCase);
    }
}
