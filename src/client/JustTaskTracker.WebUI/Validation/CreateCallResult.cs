using JustTaskTracker.WebUI.Domain.Calls.Enums;

namespace JustTaskTracker.WebUI.Validation;

public record CreateCallResult(
    string Title,
    string? Topic,
    CallVisibility Visibility,
    IReadOnlyList<Guid>? AllowedUserIds,
    IReadOnlyList<Guid>? LinkedTaskIds);
