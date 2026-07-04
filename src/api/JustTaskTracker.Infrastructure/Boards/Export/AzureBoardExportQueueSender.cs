using System.Text.Json;
using Azure.Messaging.ServiceBus;
using JustTaskTracker.Application.Common.ExternalProviders;
using JustTaskTracker.Domain.Boards.Messaging;

namespace JustTaskTracker.Infrastructure.Boards.Export;

/// <summary>
/// Sends <see cref="BoardExportMessage"/> instances to the board-archiving Service Bus queue.
/// </summary>
/// <remarks>
/// Serialised with <see cref="JsonSerializer"/> defaults (PascalCase, enum as integer)
/// to match what the Azure Functions Worker Service Bus trigger deserialises.
/// </remarks>
internal sealed class AzureBoardExportQueueSender(ServiceBusSender sender) : IBoardExportQueueSender
{
    public async Task SendAsync(BoardExportMessage message, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        var body = JsonSerializer.SerializeToUtf8Bytes(message);
        var sbMessage = new ServiceBusMessage(body)
        {
            ContentType = "application/json",
            MessageId = message.CorrelationId,
        };

        await sender.SendMessageAsync(sbMessage, ct);
    }
}
