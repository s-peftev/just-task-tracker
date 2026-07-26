using JustTaskTracker.WebUI.Domain.Calls.Enums;

namespace JustTaskTracker.WebUI.Domain.Calls;

public record CreateCallRequest(Guid BoardId, string Title, string? Topic, CallVisibility Visibility);
