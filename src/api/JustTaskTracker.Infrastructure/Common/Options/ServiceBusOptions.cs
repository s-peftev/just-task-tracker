using JustTaskTracker.Infrastructure.Common.Constants;

namespace JustTaskTracker.Infrastructure.Common.Options;

public class ServiceBusOptions
{
    public ServiceBusQueueNamesOptions? QueueNames { get; set; }

    public void Validate()
    {
        var section = ConfigSections.ServiceBus;

        if (QueueNames is null)
            throw new InvalidOperationException($"{section}:QueueNames is not configured.");

        QueueNames.Validate($"{section}:QueueNames");
    }
}

public class ServiceBusQueueNamesOptions
{
    public string BoardArchivingQueueName { get; set; } = string.Empty;

    internal void Validate(string sectionPath)
    {
        if (string.IsNullOrWhiteSpace(BoardArchivingQueueName))
            throw new InvalidOperationException($"{sectionPath}:BoardArchivingQueueName is not configured.");
    }
}
