using System.Reflection;
using DbUp;
using Microsoft.Extensions.Configuration;

const string connectionStringKey = "JustTaskTracker";
const string scriptsNamespacePrefix = "JustTaskTracker.Database.Scripts.";

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")}.json", optional: true, reloadOnChange: false)
    .AddEnvironmentVariables()
    .Build();

var connectionString =
    GetConnectionStringFromArgs(args)
    ?? configuration.GetConnectionString(connectionStringKey)
    ?? throw new InvalidOperationException(
        $"Connection string '{connectionStringKey}' was not found. " +
        $"Set it in appsettings.json, environment variable ConnectionStrings__{connectionStringKey}, " +
        "or pass --connection-string \"...\"");

Console.WriteLine("JustTaskTracker database migration");
Console.WriteLine($"Target: {MaskConnectionString(connectionString)}");
Console.WriteLine();

EnsureDatabase.For.SqlDatabase(connectionString);

var upgrader = DeployChanges.To
    .SqlDatabase(connectionString)
    .WithScriptsEmbeddedInAssembly(
        Assembly.GetExecutingAssembly(),
        script => script.StartsWith(scriptsNamespacePrefix, StringComparison.Ordinal))
    .JournalToSqlTable("dbo", "SchemaVersions")
    .LogScriptOutput()
    .LogToConsole()
    .Build();

var pendingScripts = upgrader.GetScriptsToExecute();
if (pendingScripts.Count == 0)
{
    Console.WriteLine("Database is up to date. No scripts to execute.");
    return 0;
}

Console.WriteLine($"Executing {pendingScripts.Count} script(s):");
foreach (var script in pendingScripts)
{
    Console.WriteLine($"  - {script.Name}");
}

Console.WriteLine();

var result = upgrader.PerformUpgrade();

if (!result.Successful)
{
    Console.Error.WriteLine(result.Error);
    return 1;
}

Console.WriteLine();
Console.WriteLine("Migration completed successfully.");
return 0;

static string? GetConnectionStringFromArgs(string[] args)
{
    for (var i = 0; i < args.Length; i++)
    {
        if (args[i] is "--connection-string" or "-c" && i + 1 < args.Length)
        {
            return args[i + 1];
        }
    }

    return null;
}

static string MaskConnectionString(string connectionString)
{
    var parts = connectionString.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    for (var i = 0; i < parts.Length; i++)
    {
        if (parts[i].StartsWith("Password=", StringComparison.OrdinalIgnoreCase)
            || parts[i].StartsWith("Pwd=", StringComparison.OrdinalIgnoreCase))
        {
            parts[i] = parts[i].Split('=')[0] + "=***";
        }
    }

    return string.Join(';', parts);
}
