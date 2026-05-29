using JustTaskTracker.API.Filters;
using JustTaskTracker.Application.DI;
using JustTaskTracker.Infrastructure.Common.Constants;
using JustTaskTracker.Infrastructure.DI;
using JustTaskTracker.Persistence.DI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiResponseEnvelopeFilter>();
});

builder.Services.AddApplication();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler(_ => { });
app.UseHttpsRedirection();
app.UseCors(CorsPolicies.DefaultCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
