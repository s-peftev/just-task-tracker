using JustTaskTracker.API.Extensions;
using JustTaskTracker.Application.Common.Constants;
using JustTaskTracker.Application.Users.Commands;
using JustTaskTracker.Application.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JustTaskTracker.API.Controllers;

[Route("users")]
[ApiController]
[Authorize(Policy = AuthorizationPolicies.IsAppMember)]
public class UsersController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] GetUsersForBoardLookupQuery request, CancellationToken ct)
    {
        var result = await sender.Send(request, ct);

        return result.Match(
            data => Ok(data),
            error => error.CreateErrorResponse());
    }

    [HttpPut("profile-photo")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadProfilePhoto(IFormFile photo, CancellationToken ct)
    {
        var result = await sender.Send(new UploadProfilePhotoCommand(photo), ct);

        return result.Match(
            data => Ok(data),
            error => error.CreateErrorResponse());
    }
}
