using JustTaskTracker.Domain.Boards.Messaging;

namespace JustTaskTracker.Application.Common.ExternalProviders;

public interface IBoardExportQueueSender
{
    Task SendAsync(BoardExportMessage message, CancellationToken ct = default);
}
