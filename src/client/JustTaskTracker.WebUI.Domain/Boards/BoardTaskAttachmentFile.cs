namespace JustTaskTracker.WebUI.Domain.Boards;

public record BoardTaskAttachmentFile(
    byte[] Content,
    string ContentType,
    string FileName);
