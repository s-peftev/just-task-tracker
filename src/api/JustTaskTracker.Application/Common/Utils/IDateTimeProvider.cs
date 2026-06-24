namespace JustTaskTracker.Application.Common.Utils;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
