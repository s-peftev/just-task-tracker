namespace JustTaskTracker.WebUI.Domain.Auth;

public static class ProfilePhotoValidationDisplay
{
    private static readonly Dictionary<string, string> MimeLabels = new(StringComparer.OrdinalIgnoreCase)
    {
        ["image/png"] = "PNG",
        ["image/jpeg"] = "JPEG",
        ["image/webp"] = "WebP",
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

    public static string ToAcceptAttribute(IEnumerable<string> contentTypes) =>
        string.Join(',', contentTypes.Distinct(StringComparer.OrdinalIgnoreCase));

    private static string FormatContentType(string contentType) =>
        MimeLabels.TryGetValue(contentType, out var label) ? label : contentType;
}
