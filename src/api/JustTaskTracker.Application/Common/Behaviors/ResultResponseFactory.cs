using JustTaskTracker.Domain.Common.Results;
using System.Reflection;

namespace JustTaskTracker.Application.Common.Behaviors;

internal static class ResultResponseFactory
{
    internal static TResponse CreateFailure<TResponse>(Error error)
        where TResponse : Result
    {
        if (typeof(TResponse) == typeof(Result))
            return (TResponse)(object)Result.Failure(error);

        var failureMethod = typeof(TResponse).GetMethod(
            nameof(Result<object>.Failure),
            BindingFlags.Public | BindingFlags.Static,
            [typeof(Error)])!;

        return (TResponse)failureMethod.Invoke(null, [error])!;
    }
}
