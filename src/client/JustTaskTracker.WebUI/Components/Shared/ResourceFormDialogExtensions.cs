using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace JustTaskTracker.WebUI.Components.Shared;

public static class ResourceFormDialogExtensions
{
    public static Task<IDialogReference> ShowResourceFormAsync(
        this IDialogService dialogService,
        string title,
        string submitLabel,
        string formId,
        RenderFragment formContent,
        string cancelLabel = "Cancel",
        string submitButtonClass = "btn-glass btn-dialog-action",
        ResourceFormDialogState? submitState = null)
    {
        var parameters = new DialogParameters<ResourceFormDialog>
        {
            { x => x.FormId, formId },
            { x => x.SubmitLabel, submitLabel },
            { x => x.CancelLabel, cancelLabel },
            { x => x.SubmitButtonClass, submitButtonClass },
            { x => x.ChildContent, formContent },
            { x => x.SubmitState, submitState },
        };

        return dialogService.ShowAsync<ResourceFormDialog>(
            title,
            parameters,
            ResourceFormDialog.DefaultOptions);
    }

    public static Task<IDialogReference> ShowConfirmResourceAsync(
        this IDialogService dialogService,
        string title,
        string message,
        string confirmLabel = "Confirm",
        string cancelLabel = "Cancel",
        string submitButtonClass = "btn-dialog-danger btn-dialog-action")
    {
        RenderFragment content = builder =>
        {
            builder.OpenElement(0, "p");
            builder.AddAttribute(1, "class", "confirm-dialog__message");
            builder.AddContent(2, message);
            builder.CloseElement();
        };

        return dialogService.ShowResourceFormAsync(
            title,
            confirmLabel,
            formId: string.Empty,
            formContent: content,
            cancelLabel: cancelLabel,
            submitButtonClass: submitButtonClass);
    }
}
