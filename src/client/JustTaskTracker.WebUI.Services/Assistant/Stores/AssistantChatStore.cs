using JustTaskTracker.WebUI.Domain.Assistant;
using JustTaskTracker.WebUI.Domain.Assistant.Requests;
using JustTaskTracker.WebUI.Services.Abstractions.Assistant;
using JustTaskTracker.WebUI.Services.Configuration;
using JustTaskTracker.WebUI.Services.Exceptions;

namespace JustTaskTracker.WebUI.Services.Assistant.Stores;

/// <summary>
/// App-scoped chat store: survives navigation between pages, cleared on full reload / Reset (logout).
/// </summary>
internal sealed class AssistantChatStore(
    IAssistantApiService assistantApiService,
    ValidationSettings validationSettings) : IAssistantChatStore, IDisposable
{
    private readonly List<AssistantChatMessageDto> _messages = [];
    private readonly SemaphoreSlim _sendSync = new(1, 1);

    public bool IsOpen { get; private set; }

    public bool IsSending { get; private set; }

    public string? ErrorMessage { get; private set; }

    public IReadOnlyList<AssistantChatMessageDto> Messages => _messages;

    public event Action? StateChanged;

    public void Open()
    {
        if (IsOpen)
            return;

        IsOpen = true;
        NotifyStateChanged();
    }

    public void Close()
    {
        if (!IsOpen)
            return;

        IsOpen = false;
        NotifyStateChanged();
    }

    public void Toggle()
    {
        IsOpen = !IsOpen;
        NotifyStateChanged();
    }

    public async Task SendAsync(string message, CancellationToken ct = default)
    {
        var trimmed = message.Trim();
        var maxMessageLength = validationSettings.Assistant.MaxMessageLength;
        var maxHistoryMessages = validationSettings.Assistant.MaxHistoryMessages;

        if (string.IsNullOrWhiteSpace(trimmed))
            return;

        if (trimmed.Length > maxMessageLength)
        {
            ErrorMessage = $"Message must be at most {maxMessageLength} characters.";
            NotifyStateChanged();
            return;
        }

        await _sendSync.WaitAsync(ct);
        try
        {
            if (IsSending)
                return;

            IsSending = true;
            ErrorMessage = null;

            var history = _messages
                .TakeLast(maxHistoryMessages)
                .ToArray();

            _messages.Add(new AssistantChatMessageDto(AssistantMessageRole.User, trimmed));
            NotifyStateChanged();

            try
            {
                var reply = await assistantApiService.AskAsync(
                    new AskAssistantRequest(trimmed, history),
                    ct);

                _messages.Add(new AssistantChatMessageDto(AssistantMessageRole.Assistant, reply.Answer));
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (ApiServiceException ex)
            {
                ErrorMessage = ex.Message;
            }
            catch (Exception)
            {
                ErrorMessage = "The assistant failed to generate a response.";
            }
            finally
            {
                IsSending = false;
                NotifyStateChanged();
            }
        }
        finally
        {
            _sendSync.Release();
        }
    }

    public void Reset()
    {
        IsOpen = false;
        IsSending = false;
        ErrorMessage = null;
        _messages.Clear();
        NotifyStateChanged();
    }

    public void Dispose() => _sendSync.Dispose();

    private void NotifyStateChanged() => StateChanged?.Invoke();
}
