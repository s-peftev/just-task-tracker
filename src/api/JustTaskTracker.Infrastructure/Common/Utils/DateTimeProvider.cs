using JustTaskTracker.Application.Common.Utils;

namespace JustTaskTracker.Infrastructure.Common.Utils;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
