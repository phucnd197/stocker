using Auth0.AspNetCore.Authentication.Api;
using FastEndpoints;
using FastEndpoints.Swagger;
using Stocker.Features;
using Stocker.Middleware;
using Stocker.Models.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument();
builder.Services.AddFeatureDependencies();
builder.Services.AddAuth0ApiAuthentication(options =>
{
  var config = builder.Configuration.GetSection("Auth0").Get<AuthenticationOptions>() ?? throw new ArgumentNullException("Missing configuration for authentication");
  options.Domain = config.Domain;
  options.Audience = config.Audience;
});

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();
app.UseSwaggerGen();
app.UseSwaggerUi();

app.Run();
