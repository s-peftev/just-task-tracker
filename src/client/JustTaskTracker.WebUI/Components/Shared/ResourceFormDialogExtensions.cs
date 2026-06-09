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
        string submitButtonClass = "btn-glass btn-dialog-action")
    {
        var parameters = new DialogParameters<ResourceFormDialog>
        {
            { x => x.FormId, formId },
            { x => x.SubmitLabel, submitLabel },
            { x => x.CancelLabel, cancelLabel },
            { x => x.SubmitButtonClass, submitButtonClass },
            { x => x.ChildContent, formContent }
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
        string cancelLabel = "Cancel")
    {
        RenderFragment content = builder =>
        {
            builder.OpenElement(0, "p");
            builder.AddAttribute(1, "class", "confirm-dialog__message");
            builder.AddAttribute(2, "style", "color: rgba(255,255,255,0.80); white-space: pre-line;");
            builder.AddContent(3, message);
            builder.CloseElement();
        };

        return dialogService.ShowResourceFormAsync(
            title,
            confirmLabel,
            formId: string.Empty,
            formContent: content,
            cancelLabel: cancelLabel,
            submitButtonClass: "btn-dialog-danger btn-dialog-action");
    }
}
