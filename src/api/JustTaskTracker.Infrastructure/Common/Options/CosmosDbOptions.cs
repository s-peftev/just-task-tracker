using JustTaskTracker.Infrastructure.Common.Constants;

namespace JustTaskTracker.Infrastructure.Common.Options;

public class CosmosDbOptions
{
    public string DatabaseName { get; set; } = string.Empty;

    public CosmosDbContainerNamesOptions? Containers { get; set; }

    public void Validate()
    {
        var section = ConfigSections.CosmosDB;

        if (string.IsNullOrWhiteSpace(DatabaseName))
            throw new InvalidOperationException($"{section}:DatabaseName is not configured.");

        if (Containers is null)
            throw new InvalidOperationException($"{section}:Containers is not configured.");

        Containers.Validate($"{section}:Containers");
    }
}

public class CosmosDbContainerNamesOptions
{
    public string BoardArchivalStatuses { get; set; } = string.Empty;

    internal void Validate(string sectionPath)
    {
        if (string.IsNullOrWhiteSpace(BoardArchivalStatuses))
            throw new InvalidOperationException($"{sectionPath}:BoardArchivalStatuses is not configured.");
    }
}
