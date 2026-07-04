using System.Text;

namespace JustTaskTracker.Archival.Functions.Archiving;

/// <summary>
/// Normalizes board and archive entry names for zip/blob paths.
/// </summary>
internal static class BoardArchiveNameSanitizer
{
    public static string BuildArchiveFileName(string boardName) =>
        $"{SanitizeFileName(boardName)}.zip";

    public static string SanitizeFileName(string fileName)
    {
        var name = Path.GetFileName(fileName);
        var invalidChars = Path.GetInvalidFileNameChars();
        var sb = new StringBuilder(name.Length);

        foreach (var ch in name)
            sb.Append(invalidChars.Contains(ch) ? '_' : ch);

        return sb.Length > 0 ? sb.ToString() : "attachment";
    }
}
