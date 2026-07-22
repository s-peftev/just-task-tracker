namespace JustTaskTracker.WebUI.Services.Abstractions.Theme;

public interface IThemeService
{
    string CurrentTheme { get; }

    bool IsDark { get; }

    bool IsLight { get; }

    event Action? ThemeChanged;

    Task InitializeAsync(CancellationToken ct = default);

    Task ToggleThemeAsync(CancellationToken ct = default);

    Task SetThemeAsync(string theme, CancellationToken ct = default);
}
