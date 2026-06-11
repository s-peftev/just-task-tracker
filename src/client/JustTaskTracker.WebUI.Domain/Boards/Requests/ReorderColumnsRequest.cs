namespace JustTaskTracker.WebUI.Domain.Boards.Requests;

public record ReorderColumnsRequest(IReadOnlyList<Guid> ColumnIds);
