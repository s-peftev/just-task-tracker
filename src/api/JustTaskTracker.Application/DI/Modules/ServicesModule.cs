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

        services.AddScoped<IBillingWebhookEventHandler, CustomerSubscriptionCreatedWebhookHandler>();
        services.AddScoped<IBillingWebhookEventHandler, CustomerSubscriptionUpdatedWebhookHandler>();
        services.AddScoped<IBillingWebhookEventHandler, CustomerSubscriptionDeletedWebhookHandler>();

        return services;
    }
}