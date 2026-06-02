using JustTaskTracker.Domain.Common.Enums;

namespace JustTaskTracker.Domain.Common.Results.Errors;

public static class ExceptionErrors
{
    public static readonly Error RequestCancelled = new(
        nameof(RequestCancelled),
        ErrorType.None);

    public static readonly Error Timeout = new(
        nameof(Timeout),
        ErrorType.InternalServerError);
}
