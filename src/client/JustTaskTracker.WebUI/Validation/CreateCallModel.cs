using JustTaskTracker.WebUI.Domain.Calls.Enums;

namespace JustTaskTracker.WebUI.Validation;

public class CreateCallModel
{
    public string Title { get; set; } = string.Empty;
    public string? Topic { get; set; }
    public CallVisibility Visibility { get; set; } = CallVisibility.Open;
}
