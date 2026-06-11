using System.Threading.RateLimiting;
using Auth0.AspNetCore.Authentication.Api;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;
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

builder.Services.AddAuth0ApiAuthentication(options =>
{
    var config = builder.Configuration.GetSection("Auth0").Get<AuthenticationSettings>() ??
                 throw new ArgumentNullException("Missing configuration for authentication");
    options.Domain = config.Domain;
    options.Audience = config.Audience;
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