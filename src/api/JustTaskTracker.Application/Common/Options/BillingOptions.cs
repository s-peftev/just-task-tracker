using JustTaskTracker.Application.Common.Constants;
using FeatureRegistry = JustTaskTracker.Domain.Billing.Constants.Features;

namespace JustTaskTracker.Application.Common.Options;

public class BillingOptions
{
    public Dictionary<string, PlanDefinitionOptions>? Plans { get; set; }
    public required string DefaultPlanId { get; set; }

    public void Validate()
    {
        var section = ConfigSections.Billing;

        if (Plans is null || Plans.Count == 0)
            throw new InvalidOperationException($"{section}:Plans is not configured.");

        if (string.IsNullOrWhiteSpace(DefaultPlanId))
            throw new InvalidOperationException($"{section}:{nameof(DefaultPlanId)} is not configured.");

        if (!Plans.ContainsKey(DefaultPlanId))
            throw new InvalidOperationException(
                $"{section}:{nameof(DefaultPlanId)} '{DefaultPlanId}' was not found in Plans.");

        foreach (var (planKey, plan) in Plans)
        {
            plan.Validate($"{section}:Plans:{planKey}");

            if (planKey != plan.Id)
            {
                throw new InvalidOperationException(
                    $"{section}:Plans:{planKey} key mismatch. The inner Id property must be '{planKey}', but found '{plan.Id}'.");
            }
        }
    }
}

public class PlanDefinitionOptions
{
    public required string Id { get; set; }
    public required string DisplayName { get; set; }
    public string? PriceId { get; set; }
    public string[] Features { get; set; } = [];
    public required PlanLimitsOptions Limits { get; set; }

    internal void Validate(string sectionPath)
    {
        if (string.IsNullOrWhiteSpace(Id))
            throw new InvalidOperationException($"{sectionPath}:{nameof(Id)} is not configured.");

        if (string.IsNullOrWhiteSpace(DisplayName))
            throw new InvalidOperationException($"{sectionPath}:{nameof(DisplayName)} is not configured.");

        if (Features is null)
            throw new InvalidOperationException($"{sectionPath}:{nameof(Features)} is not configured.");

        foreach (var feature in Features)
        {
            if (!FeatureRegistry.IsValid(feature))
            {
                throw new InvalidOperationException(
                    $"{sectionPath}:{nameof(Features)} contains unknown feature '{feature}'. " +
                    $"Allowed: {string.Join(", ", FeatureRegistry.GetAll())}");
            }
        }

        if (Limits is null)
            throw new InvalidOperationException($"{sectionPath}:{nameof(Limits)} is not configured.");

        Limits.Validate($"{sectionPath}:{nameof(Limits)}");
    }
}

public class PlanLimitsOptions
{
    public int? MaxBoards { get; set; }
    public int? MaxColumnsPerBoard { get; set; }
    public int? MaxTasksPerBoard { get; set; }
    public int? MaxMembersPerBoard { get; set; }

    internal void Validate(string sectionPath)
    {
        ValidateLimit(sectionPath, nameof(MaxBoards), MaxBoards);
        ValidateLimit(sectionPath, nameof(MaxColumnsPerBoard), MaxColumnsPerBoard);
        ValidateLimit(sectionPath, nameof(MaxTasksPerBoard), MaxTasksPerBoard);
        ValidateLimit(sectionPath, nameof(MaxMembersPerBoard), MaxMembersPerBoard);
    }

    private static void ValidateLimit(string sectionPath, string name, int? value)
    {
        if (value is null)
            return;

        if (value <= 0)
        {
            throw new InvalidOperationException(
                $"{sectionPath}:{name} must be greater than 0 when set (null means unlimited).");
        }
    }
}
