using JustTaskTracker.Application.Common.Constants;

namespace JustTaskTracker.Application.Common.Options;

public class ProfilePhotoProcessingSettings
{
    public ProfilePhotoOutputSettings? Originals { get; set; }

    public ProfilePhotoOutputSettings? Thumbnails { get; set; }

    public void Validate()
    {
        var section = ConfigSections.ProfilePhotoProcessing;

        if (Originals is null)
            throw new InvalidOperationException($"{section}:Originals is not configured.");

        if (Thumbnails is null)
            throw new InvalidOperationException($"{section}:Thumbnails is not configured.");

        Originals.Validate($"{section}:Originals");
        Thumbnails.Validate($"{section}:Thumbnails");
    }
}

public class ProfilePhotoOutputSettings
{
    public int Width { get; set; }

    public int Height { get; set; }

    public int WebpQuality { get; set; }

    internal void Validate(string sectionPath)
    {
        if (Width == 0)
            throw new InvalidOperationException($"{sectionPath}:Width is not configured.");

        if (Width < 0)
            throw new InvalidOperationException($"{sectionPath}:Width must be greater than 0.");

        if (Height == 0)
            throw new InvalidOperationException($"{sectionPath}:Height is not configured.");

        if (Height < 0)
            throw new InvalidOperationException($"{sectionPath}:Height must be greater than 0.");

        if (WebpQuality == 0)
            throw new InvalidOperationException($"{sectionPath}:WebpQuality is not configured.");

        if (WebpQuality is < 1 or > 100)
            throw new InvalidOperationException($"{sectionPath}:WebpQuality must be between 1 and 100.");
    }
}
