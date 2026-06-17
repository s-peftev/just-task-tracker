using System.Text.Json.Serialization;

namespace JustTaskTracker.WebUI.Domain.Boards.Requests;

public record UpdateBoardTaskAssigneeRequest(
    [property: JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    Guid? AssigneeId);
