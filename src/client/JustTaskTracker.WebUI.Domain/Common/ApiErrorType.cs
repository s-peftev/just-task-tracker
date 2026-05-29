namespace JustTaskTracker.WebUI.Domain.Common;

public enum ApiErrorType
{
    None = 0,
    NotFound = 1,
    Validation = 2,
    Unauthorized = 3,
    Conflict = 4,
    Business = 5,
    InternalServerError = 6,
    ServiceUnavailable = 7,
    Forbidden = 8
}
