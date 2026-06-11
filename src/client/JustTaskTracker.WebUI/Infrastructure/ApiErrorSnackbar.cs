using JustTaskTracker.WebUI.Services.Api;
using JustTaskTracker.WebUI.Services.Exceptions;
using MudBlazor;

namespace JustTaskTracker.WebUI.Infrastructure;

public static class ApiErrorSnackbar
{
    /// <summary>Surfaces an API failure to the user, preferring server-provided field messages.</summary>
    public static void ShowApiError(this ISnackbar snackbar, ApiServiceException ex)
    {
        var message = ApiErrorMessages.ForUser(ex.Error, ex.StatusCode);

        snackbar.Add(message, Severity.Error);
    }
}
