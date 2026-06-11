using System.Net;
using JustTaskTracker.WebUI.Domain.Common;
using JustTaskTracker.WebUI.Services.Api.Models;

namespace JustTaskTracker.WebUI.Services.Api;

public static class ApiErrorMessages
{
    public static string ForUser(ApiError? error, HttpStatusCode statusCode = default)
    {
        if (error?.Details is { Count: > 0 } details)
            return string.Join(" ", details);

        if (error is not null)
        {
            var typeMessage = error.Type switch
            {
                ApiErrorType.InternalServerError => UserErrorMessages.SomethingWentWrong,
                ApiErrorType.ServiceUnavailable => UserErrorMessages.ServiceUnavailable,
                _ => null
            };

            if (typeMessage is not null)
                return typeMessage;

            if (!string.IsNullOrWhiteSpace(error.Code))
                return error.Code;
        }

        if ((int)statusCode == 503)
            return UserErrorMessages.ServiceUnavailable;

        if ((int)statusCode >= 500)
            return UserErrorMessages.SomethingWentWrong;

        return UserErrorMessages.SomethingWentWrong;
    }
}
