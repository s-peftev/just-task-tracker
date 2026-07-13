using JustTaskTracker.Infrastructure.Common.Constants;

namespace JustTaskTracker.Infrastructure.Common.Options;

public class PaginationDefaultsOptions
{
    public int DefaultPageNumber { get; set; }
    public int DefaultPageSize { get; set; }
    public int MaxPageSize { get; set; }

    public void Validate()
    {
        var section = ConfigSections.PaginationDefaults;

        if (DefaultPageNumber == 0)
            throw new InvalidOperationException($"{section}:DefaultPageNumber is not configured.");

        if (DefaultPageNumber < 1)
            throw new InvalidOperationException($"{section}:DefaultPageNumber must be greater than 0.");

        if (DefaultPageSize == 0)
            throw new InvalidOperationException($"{section}:DefaultPageSize is not configured.");

        if (DefaultPageSize < 1)
            throw new InvalidOperationException($"{section}:DefaultPageSize must be greater than 0.");

        if (MaxPageSize == 0)
            throw new InvalidOperationException($"{section}:MaxPageSize is not configured.");

        if (MaxPageSize < 1)
            throw new InvalidOperationException($"{section}:MaxPageSize must be greater than 0.");

        if (DefaultPageSize > MaxPageSize)
            throw new InvalidOperationException($"{section}:DefaultPageSize must not exceed MaxPageSize.");
    }
}
