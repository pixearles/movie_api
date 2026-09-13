using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Movies.API.Common;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected async Task<ActionResult?> ValidateAsync<TRequest>(IValidator<TRequest> validator, TRequest request)
    {
        var validationResult = await validator.ValidateAsync(request);

        if (validationResult.IsValid)
            return null;

        validationResult.AddToModelState(ModelState);
        return ValidationProblem(ModelState);
    }
}