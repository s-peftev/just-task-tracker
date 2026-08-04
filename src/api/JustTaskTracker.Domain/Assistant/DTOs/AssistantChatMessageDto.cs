using JustTaskTracker.Domain.Assistant.Enums;

namespace JustTaskTracker.Domain.Assistant.DTOs;

public record AssistantChatMessageDto(AssistantMessageRole Role, string Content);
