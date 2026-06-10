namespace JustTaskTracker.Application.Common.Options;

public class ValidationSettings
{
    public BoardValidationSettings Boards { get; set; } = new();
}

public class BoardValidationSettings
{
    public int MaxNameSearchLength { get; set; } = 100;
}