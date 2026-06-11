using Microsoft.AspNetCore.Mvc;

namespace Stocker.Infrastructure.Web.MIddleware;

public class GlobalExceptionHandlerMiddleware
{
  private readonly RequestDelegate _next;
  private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

  public GlobalExceptionHandlerMiddleware(ILogger<GlobalExceptionHandlerMiddleware> logger, RequestDelegate next)
  {
    _logger = logger;
    _next = next;
  }


  public async Task InvokeAsync(HttpContext context)
  {
    try
    {
      await _next(context);
    }
    catch (Exception e)
    {
      var traceId = Guid.NewGuid();
      _logger.LogError(e, "Error while processing request, traceId: {traceId}", traceId);

      context.Response.StatusCode = StatusCodes.Status500InternalServerError;
      await context.Response.WriteAsJsonAsync(new ProblemDetails
      {
        Title = "Internal server error",
        Status = StatusCodes.Status500InternalServerError,
        Detail = $"Internal server error occurred, traceId: {traceId}",
      });
    }
  }
}