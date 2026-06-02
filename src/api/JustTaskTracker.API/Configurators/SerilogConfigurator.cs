using JustTaskTracker.Infrastructure.Common.Constants;
using Serilog;

namespace JustTaskTracker.API.Configurators;

public static class SerilogConfigurator
{
    private static string DefaultTemplate =>
        "[{Timestamp:HH:mm:ss} {Level:u3}] [" + LogProperties.CorrelationId + ": {" + LogProperties.CorrelationId + ":l}] {Message:lj} {NewLine}{Exception}";

    public static void Configure(IConfiguration configuration)
    {
        var loggerConfiguration = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithProperty(LogProperties.CorrelationId, LogProperties.UnknownValue);

        var hasWriteToConfig = configuration.GetSection("Serilog:WriteTo").Exists();

        if (hasWriteToConfig)
        {
            loggerConfiguration.ReadFrom.Configuration(configuration);
        }
        else
        {
            loggerConfiguration
                .WriteTo.Console(outputTemplate: DefaultTemplate)
                .ReadFrom.Configuration(configuration);
        }

        Log.Logger = loggerConfiguration.CreateLogger();
    }
}
