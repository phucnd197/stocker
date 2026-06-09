using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Stocker.Fitlers;

public class ValidationActionFilter<T> : IAsyncActionFilter where T : class
{
  private readonly IValidator<T> _validator;

  public ValidationActionFilter(IValidator<T> validator)
  {
    _validator = validator;
  }

  public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
  {
    if (!context.ModelState.IsValid)
    {
      await next();
      return;
    }
    var argument = context.ActionArguments.Values.OfType<T>().FirstOrDefault();
    if (argument is null)
    {
      await next();
      return;
    }

    var result = await _validator.ValidateAsync(argument);
    if (!result.IsValid)
    {
      var errors = result.Errors.GroupBy(x => x.PropertyName).ToDictionary(x => x.Key, x => x.Select(x => x.ErrorMessage).ToArray());
      context.Result = new BadRequestObjectResult(new { Errors = errors });
      return;
    }
    await next();
  }
}