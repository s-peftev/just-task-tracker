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
        RenderFragment formContent)
    {
        var parameters = new DialogParameters<ResourceFormDialog>
        {
            { x => x.FormId, formId },
            { x => x.SubmitLabel, submitLabel },
            { x => x.ChildContent, formContent }
        };

        return dialogService.ShowAsync<ResourceFormDialog>(
            title,
            parameters,
            ResourceFormDialog.DefaultOptions);
    }
}
