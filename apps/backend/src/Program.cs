using FastEndpoints;
using FastEndpoints.Swagger;
using Stocker.Features;
using Stocker.Features.StockRanking.Services;
using Stocker.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument();
builder.Services.AddFeatureDependencies();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
app.UseFastEndpoints();
app.UseSwaggerGen();
app.UseSwaggerUi();

app.Run();
