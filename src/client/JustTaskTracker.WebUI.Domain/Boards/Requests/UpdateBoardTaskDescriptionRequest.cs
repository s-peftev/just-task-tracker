using System.Text.Json.Serialization;

namespace JustTaskTracker.WebUI.Domain.Boards.Requests;

public record UpdateBoardTaskDescriptionRequest(
    [property: JsonIgnore(Condition = JsonIgnoreCondition.Never)]
    string? Description);
