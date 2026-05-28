using JustTaskTracker.Domain.Common.Enums;

namespace JustTaskTracker.Domain.Common.Results.Errors;

public static class GeneralErrors
{
    public static readonly Error NotFound = new(
        nameof(NotFound),
        ErrorType.NotFound);

    public static readonly Error InvalidRequest = new(
        nameof(InvalidRequest),
        ErrorType.Validation);

    public static readonly Error Unauthorized = new(
        nameof(Unauthorized),
        ErrorType.Unauthorized);

    public static readonly Error Conflict = new(
        nameof(Conflict),
        ErrorType.Conflict);

    public static readonly Error BusinessLogicError = new(
        nameof(BusinessLogicError),
        ErrorType.Business);

    public static readonly Error InternalServerError = new(
        nameof(InternalServerError),
        ErrorType.InternalServerError);

    public static readonly Error ServiceUnavailable = new(
        nameof(ServiceUnavailable),
        ErrorType.ServiceUnavailable);

    public static readonly Error Forbidden = new(
        nameof(Forbidden),
        ErrorType.Forbidden);
}
