using FluentValidation;
using JustTaskTracker.Application.Auth;
using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Common.Options;
using JustTaskTracker.Application.Common.Persistence;
using JustTaskTracker.Application.Users.ProfilePhotos;
using JustTaskTracker.Domain.Auth.DTOs;
using JustTaskTracker.Domain.Common.Results;
using JustTaskTracker.Domain.Common.Results.Errors;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace JustTaskTracker.Application.Users.Commands;

public record UploadProfilePhotoCommand(IFormFile? Photo) : IRequest<Result<ProfilePhotoDto>>;

public class UploadProfilePhotoCommandHandler(
    ICurrentUserAccessor currentUser,
    IUserRepository userRepository,
    IProfilePhotoService profilePhotoService,
    IUnitOfWork unitOfWork,
    ILogger<UploadProfilePhotoCommandHandler> logger)
    : IRequestHandler<UploadProfilePhotoCommand, Result<ProfilePhotoDto>>
{
    public async Task<Result<ProfilePhotoDto>> Handle(UploadProfilePhotoCommand request, CancellationToken ct)
    {
        var user = await userRepository.GetUserByAzureAOIAsync(currentUser.AzureAdObjectId, ct);

        if (user is null)
            return Result<ProfilePhotoDto>.Failure(GeneralErrors.Unauthorized);

        var version = GenerateVersion();
        var photo = request.Photo!;

        await using var stream = photo.OpenReadStream();

        try
        {
            await profilePhotoService.UploadPhotoAsync(user.Id, stream, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to upload profile photo for user {UserId}.", user.Id);
            return Result<ProfilePhotoDto>.Failure(GeneralErrors.ServiceUnavailable);
        }

        user.ProfilePhotoVersion = version;

        await unitOfWork.SaveChangesAsync(ct);

        var url = profilePhotoService.BuildOriginalUrl(user.Id, version);

        return Result<ProfilePhotoDto>.Success(new ProfilePhotoDto(url));
    }

    private static string GenerateVersion() =>
        DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
}

public class UploadProfilePhotoCommandValidator : AbstractValidator<UploadProfilePhotoCommand>
{
    public UploadProfilePhotoCommandValidator(ValidationSettings validationSettings)
    {
        var allowedContentTypes = new HashSet<string>(
            validationSettings.ProfilePhotos!.AllowedContentTypes!,
            StringComparer.OrdinalIgnoreCase);

        RuleFor(x => x.Photo)
            .NotNull()
            .WithMessage("'Photo' must not be empty.");

        When(x => x.Photo is not null, () =>
        {
            RuleFor(x => x.Photo!.ContentType)
                .NotEmpty()
                .Must(contentType => allowedContentTypes.Contains(contentType))
                .WithMessage("'ContentType' is not allowed.");

            RuleFor(x => x.Photo!.Length)
                .GreaterThan(0)
                .LessThanOrEqualTo(validationSettings.ProfilePhotos!.MaxPhotoSizeBytes);
        });
    }
}
