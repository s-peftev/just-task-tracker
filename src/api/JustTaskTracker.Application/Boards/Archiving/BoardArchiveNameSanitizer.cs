using System.Text;

namespace JustTaskTracker.Application.Boards.Archiving;

/// <summary>
/// Normalizes board archive file names for blob storage paths.
/// </summary>
/// <remarks>
/// Sanitization rules must stay in sync with
/// <c>JustTaskTracker.Archival.Functions.Archiving.BoardArchiveNameSanitizer</c>.
/// </remarks>
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
