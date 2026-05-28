using JustTaskTracker.Domain.Common.Results;

namespace JustTaskTracker.API.Models;

public class ApiResponse<T>
{
    public bool Success { get; init; }
    public T? Data { get; init; }
    public Error? Error { get; init; }

    private ApiResponse(T? data, bool success, Error? error = null)
    {
        Data = data;
        Success = success;
        Error = error;
    }

    public static ApiResponse<T> Ok(T? data = default) =>
        new(data, true);

    public static ApiResponse<T> Fail(Error error) =>
        new(default, false, error);
}
