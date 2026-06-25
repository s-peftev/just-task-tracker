using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Common.Persistence;
using JustTaskTracker.Application.Users.ProfilePhotos;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;
using Microsoft.Extensions.Logging;

namespace JustTaskTracker.Application.Users.Commands;

public record DeleteProfilePhotoCommand() : IRequest<Result>;

public class DeleteProfilePhotoCommandHandler(
    ICurrentUserAccessor currentUser,
    IUserRepository userRepository,
    IProfilePhotoService profilePhotoService,
    IUnitOfWork unitOfWork,
    ILogger<DeleteProfilePhotoCommandHandler> logger)
    : IRequestHandler<DeleteProfilePhotoCommand, Result>
{
    public async Task<Result> Handle(DeleteProfilePhotoCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetUserByAzureAOIAsync(currentUser.AzureAdObjectId, ct);

        if (user is null)
            return Result.Failure(GeneralErrors.Unauthorized);

        if (string.IsNullOrEmpty(user.ProfilePhotoVersion))
            return Result.Success();

        user.ProfilePhotoVersion = null;

        await unitOfWork.SaveChangesAsync(ct);

        try
        {
            await profilePhotoService.DeleteProfilePhotoAsync(user.Id, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Profile photo version cleared for user {UserId}, but blobs could not be deleted.",
                user.Id);
        }

        return Result.Success();
    }
}
