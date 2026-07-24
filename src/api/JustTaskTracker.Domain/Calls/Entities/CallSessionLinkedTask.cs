namespace JustTaskTracker.Domain.Calls.Entities;

// Stub for Story 1.3 (optional task linking, AD-13). Not yet EF-mapped or persisted.
// TaskId references BoardTask.Id.
public class CallSessionLinkedTask
{
    public Guid CallSessionId { get; set; }
    public Guid TaskId { get; set; }
}
