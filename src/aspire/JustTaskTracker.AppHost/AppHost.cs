var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql")
    .WithDataVolume("jtt-sql-data")
    .WithLifetime(ContainerLifetime.Persistent);

var database = sql.AddDatabase("MSQLDb");

var migrations = builder.AddProject<Projects.JustTaskTracker_Database>("migrations")
    .WithReference(database)
    .WaitFor(database);

var api = builder.AddProject<Projects.JustTaskTracker_API>("api")
    .WithReference(database)
    .WaitFor(database)
    .WaitForCompletion(migrations)
    .WithUrlForEndpoint("https", _ => new()
    {
        Url = "/hangfire",
        DisplayText = "Hangfire",
    });

builder.AddProject<Projects.JustTaskTracker_WebUI>("webui");

builder.AddAzureFunctionsProject<Projects.JustTaskTracker_Archival_Functions>("archival-functions");

builder.Build().Run();
