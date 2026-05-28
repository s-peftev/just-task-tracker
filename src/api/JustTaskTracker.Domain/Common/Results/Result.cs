using JustTaskTracker.Domain.Common.Constants.ExceptionMessages;

namespace JustTaskTracker.Domain.Common.Results;

/// <summary>
/// Represents the outcome of an operation that can either succeed or fail.
/// Use <see cref="Match{TResult}"/> to handle both cases without checking <see cref="IsSuccess"/> explicitly.
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    private readonly Error? _error;

    /// <summary>The error describing why the operation failed. Throws if the result is a success.</summary>
    public Error Error => IsSuccess
        ? throw new InvalidOperationException(ResultExceptionMessages.CannotAccessErrorOnSuccess)
        : _error!;

    protected Result(bool isSuccess, Error? error)
    {
        if (isSuccess && error != null)
            throw new InvalidOperationException(ResultExceptionMessages.SuccessMustNotHaveError);
        if (!isSuccess && error == null)
            throw new InvalidOperationException(ResultExceptionMessages.FailureMustHaveError);

        IsSuccess = isSuccess;
        _error = error;
    }

    /// <summary>Creates a successful result with no attached error.</summary>
    public static Result Success() => new(true, null);

    /// <summary>Creates a failed result carrying the given <paramref name="error"/>.</summary>
    public static Result Failure(Error error) => new(false, error);

    /// <summary>
    /// Executes <paramref name="onSuccess"/> if the result is successful,
    /// otherwise executes <paramref name="onFailure"/> with the error.
    /// Avoids explicit <see cref="IsSuccess"/> checks at call sites.
    /// </summary>
    public TResult Match<TResult>(Func<TResult> onSuccess, Func<Error, TResult> onFailure) =>
        IsSuccess ? onSuccess() : onFailure(Error);
}

/// <summary>
/// Represents the outcome of an operation that returns a value of type <typeparamref name="T"/> on success.
/// Use <see cref="Match{TResult}"/> to handle both cases without checking <see cref="Result.IsSuccess"/> explicitly.
/// </summary>
public class Result<T> : Result
{
    private readonly T? _value;

    /// <summary>The value produced by the operation. Throws if the result is a failure.</summary>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException(ResultExceptionMessages.CannotAccessValueOnFailure);

    private Result(T value) : base(true, null)
    {
        _value = value
            ?? throw new ArgumentNullException(nameof(value), ResultExceptionMessages.SuccessCannotHaveNullValue);
    }

    private Result(Error error) : base(false, error)
    {
        _value = default;
    }

    public static Result<T> Success(T value) => new(value);

    public static new Result<T> Failure(Error error) => new(error);

    /// <summary>
    /// Executes <paramref name="onSuccess"/> with the result value if successful,
    /// otherwise executes <paramref name="onFailure"/> with the error.
    /// Avoids explicit <see cref="Result.IsSuccess"/> checks at call sites.
    /// </summary>
    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<Error, TResult> onFailure) =>
        IsSuccess ? onSuccess(Value) : onFailure(Error);
}
