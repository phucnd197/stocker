using FastEndpoints;
using FastEndpoints.Swagger;
using Stocker.Features;
using Stocker.Features.StockRanking.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFastEndpoints();
builder.Services.SwaggerDocument();
builder.Services.AddFeatureDependencies();

var app = builder.Build();

app.UseFastEndpoints();
app.UseSwaggerGen();
app.UseSwaggerUi();

app.Run();
