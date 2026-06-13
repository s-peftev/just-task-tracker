using JustTaskTracker.Domain.Boards.Constants;

namespace JustTaskTracker.Application.Common.Options;

public class ValidationSettings
{
    public BoardValidationSettings Boards { get; set; } = new();
}

public class BoardValidationSettings
{
    public int MaxNameSearchLength { get; set; } = BoardFieldLengths.MaxNameLength;
}
