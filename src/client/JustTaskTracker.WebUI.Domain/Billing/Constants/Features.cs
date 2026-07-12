namespace JustTaskTracker.WebUI.Domain.Billing.Constants;

public static class Features
{
    public const string BoardExport = "board.export";
    public const string BoardReExport = "board.reexport";
    public const string BoardArchiveDownload = "board.archive.download";

    private static readonly HashSet<string> All =
    [
        BoardExport,
        BoardReExport,
        BoardArchiveDownload,
    ];

    public static bool IsValid(string? feature) =>
        !string.IsNullOrEmpty(feature) && All.Contains(feature);

    public static IReadOnlyCollection<string> GetAll() => All;
}
