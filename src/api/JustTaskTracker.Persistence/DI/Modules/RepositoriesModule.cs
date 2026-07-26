using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Billing.Repositories;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Application.Calls.Repositories;
using JustTaskTracker.Application.Common.Persistence;
using JustTaskTracker.Persistence.Auth.Repositories;
using JustTaskTracker.Persistence.Billing.Repositories;
using JustTaskTracker.Persistence.Boards.Repositories;
using JustTaskTracker.Persistence.Calls.Repositories;
using JustTaskTracker.Persistence.Common;
using Microsoft.Extensions.DependencyInjection;

namespace JustTaskTracker.Persistence.DI.Modules;

internal static class RepositoriesModule
{
    public static IServiceCollection AddRepositoriesModule(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IBoardRepository, BoardRepository>();
        services.AddScoped<IColumnRepository, ColumnRepository>();
        services.AddScoped<IBoardTaskRepository, BoardTaskRepository>();
        services.AddScoped<IBoardTaskCommentRepository, BoardTaskCommentRepository>();
        services.AddScoped<IAttachmentRepository, AttachmentRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IStripeWebhookEventRepository, StripeWebhookEventRepository>();
        services.AddScoped<ICallRepository, CallRepository>();
        services.AddScoped<IAcsUserIdentityMappingRepository, AcsUserIdentityMappingRepository>();

        return services;
    }
}
