using JustTaskTracker.Application.Auth.Repositories;
using JustTaskTracker.Application.Boards.Repositories;
using JustTaskTracker.Persistence.Auth.Repositories;
using JustTaskTracker.Persistence.Common;
using JustTaskTracker.Persistence.Boards.Repositories;
using Microsoft.Extensions.DependencyInjection;
using JustTaskTracker.Application.Common.Persistence;

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

        return services;
    }
}
