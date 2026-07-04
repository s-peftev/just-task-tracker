using JustTaskTracker.Application.Common.Constants;

namespace JustTaskTracker.Application.Common.Options;

public class BoardExportSchedulerOptions
{
    public required string CronExpression { get; set; }

    /// <summary>
    /// Maximum number of Cosmos DB documents to process per job run.
    /// </summary>
    public required int DocumentBatchSize { get; set; }

    /// <summary>
    /// How many minutes must elapse after the last <c>updatedAtUtc</c> timestamp
    /// before a <c>Failed</c> export document is eligible for a retry.
    /// </summary>
    public required int FailedRetryCooldownMinutes { get; set; }

    public void Validate()
    {
        var section = ConfigSections.BoardExportScheduler;

        if (string.IsNullOrWhiteSpace(CronExpression))
            throw new InvalidOperationException($"{section}:{nameof(CronExpression)} is not configured.");

        if (DocumentBatchSize <= 0)
            throw new InvalidOperationException(
                $"{section}:{nameof(DocumentBatchSize)} must be greater than 0.");

        if (FailedRetryCooldownMinutes < 0)
            throw new InvalidOperationException(
                $"{section}:{nameof(FailedRetryCooldownMinutes)} must be greater than or equal to 0.");
    }
}
