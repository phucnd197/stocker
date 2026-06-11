using System.Threading.RateLimiting;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Stocker.Core.Settings;
using Stocker.Features;
using Stocker.Infrastructure;
using Stocker.Infrastructure.Web.Middleware;
using Stocker.Infrastructure.Web.Startup;
using TickerQ.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument();
builder.Services.AddFeatureDependencies();
builder.Services.AddCors();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var config = builder.Configuration.GetSection("Auth0").Get<AuthenticationSettings>() ??
                 throw new ArgumentNullException("Missing configuration for authentication");
    options.Authority = config.Domain;
    options.Audience = config.Audience;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // Ensure the token issuer matches your exact Auth0 Tenant Domain
        ValidateIssuer = true,
        ValidIssuer = config.Domain,

        // Ensure the token was intended for your specific API
        ValidateAudience = true,
        ValidAudience = config.Audience,

        // Validate that the token has not expired yet
        ValidateLifetime = true,

        // Auth0 signs tokens cryptographically using RS256 via a public JWKS endpoint.
        // The middleware fetches these keys automatically from your Authority domain.
        ValidateIssuerSigningKey = true
    };
});

builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 1000,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));

    options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsJsonAsync(new { error = "Too many requests!" }, token);
    };
});

var minioConfig = builder.Configuration.GetSection("Minio").Get<MinioSettings>() ??
                  throw new ArgumentException("Missing Minio configuration");

var serviceName = "Stocker";
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName))
    .WithMetrics(metrics =>
    {
        metrics.AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter();
    })
    .WithTracing(tracing =>
    {
        tracing.AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddOtlpExporter();
    });
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddOpenTelemetry(options =>
{
    options.IncludeScopes = true;
    options.IncludeFormattedMessage = true;
    options.AddOtlpExporter();
});

var app = builder.Build();
app.UseCors(policy =>
{
    var origins = builder.Configuration.GetSection("Cors").Get<string[]>() ??
                  throw new ArgumentNullException("Missing cors configuration");
    policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
});
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();
app.UseSwaggerGen();
app.UseSwaggerUi();
app.UseTickerQ();

await EnsureBucketCreation.RunAsync(minioConfig, app.Services);

app.Run();