using JustTaskTracker.Persistence.Common;
using JustTaskTracker.Persistence.Common.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JustTaskTracker.Persistence.DI.Modules;

internal static class DatabaseModule
{
    public static IServiceCollection AddDatabaseModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringNames.JustTaskTracker)
            ?? throw new InvalidOperationException("DB connection string is not configured.");

        services.AddDbContext<JustTaskTrackerDbContext>(options =>
            options.UseSqlServer(connectionString));

        return services;
    }
}
