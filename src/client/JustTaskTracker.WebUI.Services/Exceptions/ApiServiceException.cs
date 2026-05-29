using JustTaskTracker.WebUI.Services.Api.Models;
using System.Net;

namespace JustTaskTracker.WebUI.Services.Exceptions;

public class ApiServiceException(HttpStatusCode statusCode, ApiError? error, string message)
    : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
    public ApiError? Error { get; } = error;
}
