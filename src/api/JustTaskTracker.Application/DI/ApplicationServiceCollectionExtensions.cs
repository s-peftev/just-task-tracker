using FluentValidation;
using JustTaskTracker.Application.Boards.Positioning;
using JustTaskTracker.Application.Common.Behaviors;
using JustTaskTracker.Application.Common.Constants;
using JustTaskTracker.Application.Common.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace JustTaskTracker.Application.DI;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        var assembly = typeof(ApplicationServiceCollectionExtensions).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        ValidatorOptions.Global.LanguageManager.Enabled = false;
        services.AddValidatorsFromAssembly(assembly);

        var validationSettingsOptions = configuration
            .GetSection(ConfigSections.ValidationSettings)
            .Get<ValidationSettings>() ?? new ValidationSettings();

        validationSettingsOptions.BoardTasks.ApplyDefaultsIfMissing();

        services.AddSingleton(validationSettingsOptions);

        services.AddScoped<IBoardPositioningService, BoardPositioningService>();

        return services;
    }
}
