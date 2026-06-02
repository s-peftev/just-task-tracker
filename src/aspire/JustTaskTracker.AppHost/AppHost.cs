var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sql")
    .WithDataVolume("jtt-sql-data")
    .WithLifetime(ContainerLifetime.Persistent);

var database = sql.AddDatabase("JustTaskTracker");

var migrations = builder.AddProject<Projects.JustTaskTracker_Database>("migrations")
    .WithReference(database)
    .WaitFor(database);

var api = builder.AddProject<Projects.JustTaskTracker_API>("api")
    .WithReference(database)
    .WaitFor(database)
    .WaitForCompletion(migrations);

builder.AddProject<Projects.JustTaskTracker_WebUI>("webui");

builder.Build().Run();
