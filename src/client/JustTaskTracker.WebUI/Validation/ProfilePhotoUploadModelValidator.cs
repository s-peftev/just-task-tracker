using FluentValidation;
using JustTaskTracker.WebUI.Domain.Auth;
using JustTaskTracker.WebUI.Services.Configuration;

namespace JustTaskTracker.WebUI.Validation;

public class ProfilePhotoUploadModelValidator : AbstractValidator<ProfilePhotoUploadModel>
{
    public ProfilePhotoUploadModelValidator(ValidationSettings validationSettings)
    {
        var profilePhotos = validationSettings.ProfilePhotos;

        var allowedContentTypes = new HashSet<string>(
            profilePhotos.AllowedContentTypes,
            StringComparer.OrdinalIgnoreCase);

        RuleFor(x => x.ContentType)
            .NotEmpty()
            .WithMessage("Photo type is not supported.")
            .Must(contentType => allowedContentTypes.Contains(contentType))
            .WithMessage("Photo type is not supported.");

        RuleFor(x => x.FileSizeBytes)
            .GreaterThan(0)
            .WithMessage("The selected photo is empty.")
            .LessThanOrEqualTo(profilePhotos.MaxPhotoSizeBytes)
            .WithMessage(
                $"Photo size must not exceed {ProfilePhotoValidationDisplay.FormatMaxFileSize(profilePhotos.MaxPhotoSizeBytes)}.");
    }
}
