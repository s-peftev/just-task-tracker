namespace JustTaskTracker.Application.Common.Behaviors;

/// <summary>
/// Marks a MediatR request that requires a billing feature entitlement.
/// </summary>
public interface IRequireFeature
{
    string Feature { get; }
}
