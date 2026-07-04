using System.Text;
using JustTaskTracker.Archival.Functions.Archiving.Summary;

namespace JustTaskTracker.Archival.Functions.Archiving;

/// <summary>
/// Builds sanitized, deduplicated paths for board export archives.
/// </summary>
internal sealed class BoardArchivePathBuilder
{
    private readonly Dictionary<Guid, string> _taskFolderByTaskId = new();
    private readonly Dictionary<string, int> _taskTitleUsageCounts = new(StringComparer.OrdinalIgnoreCase);
    private readonly HashSet<string> _usedEntryPaths = new(StringComparer.OrdinalIgnoreCase);

    public string BuildArchiveFileName(string boardName) =>
        $"{SanitizeFileName(boardName)}.zip";

    public string BuildAttachmentEntryPath(Guid taskId, string taskTitle, int position, string originalFileName)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(taskId, Guid.Empty);
        ArgumentException.ThrowIfNullOrWhiteSpace(originalFileName);

        var taskFolder = AllocateTaskFolderName(taskId, taskTitle);
        var entryPath =
            $"{BoardArchiveEntryNames.AttachmentsFolder}/{taskFolder}/{position:000}_{SanitizeFileName(originalFileName)}";

        return EnsureUniqueEntryPath(entryPath);
    }

    /// <summary>
    /// Returns a unique attachment folder name for the given task within this archive build.
    /// </summary>
    /// <remarks>
    /// All attachments of the same task share one folder. Different tasks with the same sanitized title
    /// receive a numeric suffix (<c>title_2</c>, <c>title_3</c>, …).
    /// </remarks>
    private string AllocateTaskFolderName(Guid taskId, string taskTitle)
    {
        if (_taskFolderByTaskId.TryGetValue(taskId, out var existingFolder))
            return existingFolder;

        var baseName = SanitizePathSegment(taskTitle);

        if (!_taskTitleUsageCounts.TryGetValue(baseName, out var count))
        {
            _taskTitleUsageCounts[baseName] = 1;
            _taskFolderByTaskId[taskId] = baseName;
            return baseName;
        }

        count++;
        _taskTitleUsageCounts[baseName] = count;

        var folder = $"{baseName}_{count}";
        _taskFolderByTaskId[taskId] = folder;
        return folder;
    }

    private string EnsureUniqueEntryPath(string entryPath)
    {
        if (_usedEntryPaths.Add(entryPath))
            return entryPath;

        var directory = Path.GetDirectoryName(entryPath)!.Replace('\\', '/');
        var fileName = Path.GetFileName(entryPath);

        for (var suffix = 2; ; suffix++)
        {
            var candidate = $"{directory}/{AppendNumericSuffix(fileName, suffix)}";

            if (_usedEntryPaths.Add(candidate))
                return candidate;
        }
    }

    private static string AppendNumericSuffix(string fileName, int suffix)
    {
        var extension = Path.GetExtension(fileName);
        var stem = Path.GetFileNameWithoutExtension(fileName);

        return string.IsNullOrEmpty(extension)
            ? $"{stem}_{suffix}"
            : $"{stem}_{suffix}{extension}";
    }

    /// <summary>
    /// Normalizes a file name for use inside a zip entry (strips path segments, replaces invalid characters).
    /// </summary>
    /// <returns>The sanitized name, or <c>attachment</c> when the input yields an empty result.</returns>
    public static string SanitizeFileName(string fileName)
    {
        var name = Path.GetFileName(fileName);
        var invalidChars = Path.GetInvalidFileNameChars();
        var sb = new StringBuilder(name.Length);

        foreach (var ch in name)
            sb.Append(invalidChars.Contains(ch) ? '_' : ch);

        return sb.Length > 0 ? sb.ToString() : "attachment";
    }

    /// <summary>
    /// Normalizes a path segment (e.g. task title or board name) for use in archive entry paths.
    /// </summary>
    /// <returns>The sanitized segment, or <c>task</c> when the input yields an empty result.</returns>
    public static string SanitizePathSegment(string segment)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sb = new StringBuilder(segment.Length);

        foreach (var ch in segment.Trim())
            sb.Append(invalidChars.Contains(ch) ? '_' : ch);

        return sb.Length > 0 ? sb.ToString() : "task";
    }
}
