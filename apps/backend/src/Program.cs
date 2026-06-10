using Auth0.AspNetCore.Authentication.Api;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.EntityFrameworkCore;
using Minio;
using Stocker.Database;
using Stocker.Database.Interceptors;
using Stocker.Features;
using Stocker.Middleware;
using Stocker.Models.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument();
builder.Services.AddFeatureDependencies();

// builder.Services.AddDbContext<StockerDataContext>(options =>
// {
//   options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")).AddInterceptors(new SoftDeleteInterceptors());
// });

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<StockerDataContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddAuth0ApiAuthentication(options =>
{
  var config = builder.Configuration.GetSection("Auth0").Get<AuthenticationOptions>() ?? throw new ArgumentNullException("Missing configuration for authentication");
  options.Domain = config.Domain;
  options.Audience = config.Audience;
});

var minioConfig = builder.Configuration.GetSection("Minio").Get<MinioOptions>() ?? throw new ArgumentException("Missing Minio configuration");
builder.Services.AddMinio(configureClient =>
{
  configureClient
  .WithEndpoint(minioConfig.Endpoint)
  .WithCredentials(minioConfig.AccessKey, minioConfig.SecretKey);
});

var app = builder.Build();
app.UseCors(policy =>
{
  var origins = builder.Configuration.GetSection("Cors").Get<string[]>() ?? throw new ArgumentNullException("Missing cors configuration");
  policy.WithOrigins(origins).AllowAnyHeader().AllowAnyMethod();
});
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();
app.UseSwaggerGen();
app.UseSwaggerUi();

app.Run();
