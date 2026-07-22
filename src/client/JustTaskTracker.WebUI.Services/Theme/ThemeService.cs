using JustTaskTracker.WebUI.Services.Abstractions.Theme;
using Microsoft.JSInterop;

namespace JustTaskTracker.WebUI.Services.Theme;

internal sealed class ThemeService(IJSRuntime js) : IThemeService
{
    public const string Dark = "dark";
    public const string Light = "light";

    private bool _initialized;

    public string CurrentTheme { get; private set; } = Dark;

    public bool IsDark => CurrentTheme == Dark;

    public bool IsLight => CurrentTheme == Light;

    public event Action? ThemeChanged;

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        if (_initialized)
            return;

        var theme = await js.InvokeAsync<string>("jttTheme.getStoredTheme", ct);
        CurrentTheme = Normalize(theme);
        _initialized = true;
        ThemeChanged?.Invoke();
    }

    public async Task ToggleThemeAsync(CancellationToken ct = default)
    {
        var next = IsDark ? Light : Dark;
        await SetThemeAsync(next, ct);
    }

    public async Task SetThemeAsync(string theme, CancellationToken ct = default)
    {
        var normalized = Normalize(theme);
        if (string.Equals(CurrentTheme, normalized, StringComparison.Ordinal))
            return;

        CurrentTheme = await js.InvokeAsync<string>("jttTheme.setTheme", ct, normalized);
        ThemeChanged?.Invoke();
    }

    private static string Normalize(string theme) =>
        string.Equals(theme, Light, StringComparison.Ordinal) ? Light : Dark;
}
