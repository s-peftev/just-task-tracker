using JustTaskTracker.Application.Assistant.Tools;
using JustTaskTracker.Application.Assistant.Tools.Handlers;
using JustTaskTracker.Application.Billing.Webhooks;
using JustTaskTracker.Application.Billing.Webhooks.Handlers;
using JustTaskTracker.Application.Boards.Attachments;
using JustTaskTracker.Application.Boards.Positioning;
using JustTaskTracker.Application.Users.ProfilePhotos;
using Microsoft.Extensions.DependencyInjection;

namespace JustTaskTracker.Application.DI.Modules;

internal static class ServicesModule
{
    internal static IServiceCollection AddServicesModule(this IServiceCollection services)
    {
        services.AddScoped<IBoardPositioningService, BoardPositioningService>();
        services.AddScoped<IBoardTaskAttachmentService, BoardTaskAttachmentService>();
        services.AddScoped<IProfilePhotoService, ProfilePhotoService>();

        services.AddScoped<IAssistantToolHandler, GetActiveOwnedBoardsCountToolHandler>();
        services.AddScoped<IAssistantToolHandler, GetRequesterAccountToolHandler>();
        services.AddScoped<IAssistantToolExecutor, AssistantToolExecutor>();

        services.AddScoped<IBillingWebhookEventHandler, CustomerSubscriptionCreatedWebhookHandler>();
        services.AddScoped<IBillingWebhookEventHandler, CustomerSubscriptionUpdatedWebhookHandler>();
        services.AddScoped<IBillingWebhookEventHandler, CustomerSubscriptionDeletedWebhookHandler>();

        return services;
    }
}