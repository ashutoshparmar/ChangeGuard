using System.Text.Json.Serialization;

using ChangeGuard.Api.Infrastructure;
using ChangeGuard.Application.ChangeRequests;
using ChangeGuard.Infrastructure;

using Microsoft.AspNetCore.Diagnostics.HealthChecks;

const string FrontendCorsPolicy = "FrontendCorsPolicy";
const string CorrelationHeader = "X-Correlation-ID";

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter()));
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddHealthChecks();
builder.Services.AddOpenApi();

builder.Services.AddScoped<ICreateChangeRequestService, CreateChangeRequestService>();
builder.Services.AddScoped<IChangeRequestQueryService, ChangeRequestQueryService>();
builder.Services.AddScoped<IChangeRequestWorkflowService, ChangeRequestWorkflowService>();
builder.Services.AddScoped<IReleaseReadinessService, ReleaseReadinessService>();
builder.Services.AddInfrastructure(builder.Configuration);

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()
    ?? ["http://localhost:4200"];

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        FrontendCorsPolicy,
        policy => policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

if (builder.Configuration.GetValue<bool>(
        "Database:ApplyMigrationsOnStartup"))
{
    await app.Services.ApplyDatabaseMigrationsAsync();
}

app.Use(async (context, next) =>
{
    if (context.Request.Headers.TryGetValue(
            CorrelationHeader,
            out var correlationId)
        && !string.IsNullOrWhiteSpace(correlationId))
    {
        context.TraceIdentifier = correlationId.ToString();
    }

    context.Response.Headers[CorrelationHeader] = context.TraceIdentifier;
    await next();
});

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors(FrontendCorsPolicy);
app.UseAuthorization();

app.MapHealthChecks(
    "/health/live",
    new HealthCheckOptions
    {
        Predicate = _ => false
    });
app.MapHealthChecks(
    "/health/ready",
    new HealthCheckOptions
    {
        Predicate = registration =>
            registration.Tags.Contains("ready")
    });
app.MapControllers();

app.Run();

public partial class Program
{
}
