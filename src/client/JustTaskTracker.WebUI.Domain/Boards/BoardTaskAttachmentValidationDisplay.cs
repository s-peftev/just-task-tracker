namespace JustTaskTracker.WebUI.Domain.Boards;

public static class BoardTaskAttachmentValidationDisplay
{
    private static readonly Dictionary<string, string> MimeLabels = new(StringComparer.OrdinalIgnoreCase)
    {
        ["application/pdf"] = "PDF",
        ["image/png"] = "PNG",
        ["image/jpeg"] = "JPEG",
        ["image/gif"] = "GIF",
        ["image/webp"] = "WebP",
        ["text/plain"] = "plain text",
        ["application/vnd.openxmlformats-officedocument.wordprocessingml.document"] = "Word (.docx)",
        ["application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"] = "Excel (.xlsx)",
        ["application/zip"] = "ZIP",
    };

    public static string FormatMaxFileSize(long bytes)
    {
        const long mb = 1024 * 1024;

        return bytes % mb == 0
            ? $"{bytes / mb} MB"
            : $"{bytes / (1024.0 * 1024.0):0.##} MB";
    }

    public static string FormatAllowedContentTypes(IEnumerable<string> contentTypes) =>
        string.Join(", ", contentTypes
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Select(FormatContentType));

    private static string FormatContentType(string contentType) =>
        MimeLabels.TryGetValue(contentType, out var label) ? label : contentType;
}
