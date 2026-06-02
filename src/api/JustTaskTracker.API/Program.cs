using JustTaskTracker.API.Configurators;
using JustTaskTracker.API.Filters;
using JustTaskTracker.API.Middleware;
using JustTaskTracker.Application.DI;
using JustTaskTracker.Infrastructure.Common.Constants;
using JustTaskTracker.Infrastructure.DI;
using JustTaskTracker.Persistence.DI;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiResponseEnvelopeFilter>();

    options.Conventions.Add(new PrefixConventionConfigurator("api"));
});

builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

SerilogConfigurator.Configure(builder.Configuration);
builder.Host.UseSerilog();

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseExceptionHandler(_ => { });
app.UseHttpsRedirection();
app.UseCors(CorsPolicies.DefaultCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
