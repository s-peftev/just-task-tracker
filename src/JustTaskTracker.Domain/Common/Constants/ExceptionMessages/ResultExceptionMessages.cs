namespace JustTaskTracker.Domain.Common.Constants.ExceptionMessages;

public static class ResultExceptionMessages
{
    public const string CannotAccessErrorOnSuccess = "Cannot access Error when result is success.";
    public const string SuccessMustNotHaveError = "A successful result must not have an error.";
    public const string FailureMustHaveError = "A failed result must have an error.";
    public const string CannotAccessValueOnFailure = "Cannot access Value when result is failure.";
    public const string SuccessCannotHaveNullValue = "Success result cannot have null value.";
}
