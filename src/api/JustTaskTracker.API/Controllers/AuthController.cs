using JustTaskTracker.API.Extensions;
using JustTaskTracker.Application.Auth.Commands;
using JustTaskTracker.Application.Auth.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JustTaskTracker.API.Controllers;

[Route("auth")]
[ApiController]
public class AuthController(ISender sender) : ControllerBase
{
    /// <summary>
    /// After Entra login (Bearer is already in the header): provision + profile.
    /// </summary>
    [Authorize]
    [HttpPost("login")]
    public async Task<IActionResult> Login(CancellationToken ct)
    {
        var result = await sender.Send(new LoginCommand(), ct);

        return result.Match(
            data => Ok(data),
            error => error.CreateErrorResponse());
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken ct)
    {
        var result = await sender.Send(new GetCurrentUserQuery(), ct);

        return result.Match(
            data => Ok(data),
            error => error.CreateErrorResponse());
    }
}
