using System.Text;

namespace JustTaskTracker.Application.Boards.Archiving;

/// <summary>
/// Builds sanitized paths for board export archives.
/// </summary>
/// <remarks>
/// Sanitization rules must stay in sync with
/// <c>JustTaskTracker.Archival.Functions.Archiving.BoardArchivePathBuilder</c>.
/// </remarks>
internal static class BoardArchivePathBuilder
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
