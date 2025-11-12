namespace UserService.API.Filters;

using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Reflection;

public class ValidationFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;

    public ValidationFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Получаем все параметры действия
        foreach (var parameter in context.ActionDescriptor.Parameters)
        {
            if (context.ActionArguments.TryGetValue(parameter.Name, out var argument))
            {
                if (argument != null)
                {
                    // Ищем валидатор для типа параметра
                    var argumentType = argument.GetType();
                    var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);
                    var validator = _serviceProvider.GetService(validatorType);

                    if (validator != null)
                    {
                        // Создаем ValidationContext через reflection
                        var validationContextType = typeof(ValidationContext<>).MakeGenericType(argumentType);
                        var validationContext = Activator.CreateInstance(validationContextType, argument);

                        // Вызываем ValidateAsync через reflection
                        var validateAsyncMethod = validatorType.GetMethod("ValidateAsync", new[] { validationContextType, typeof(System.Threading.CancellationToken) });
                        if (validateAsyncMethod != null)
                        {
                            var task = validateAsyncMethod.Invoke(validator, new[] { validationContext, CancellationToken.None }) as Task<FluentValidation.Results.ValidationResult>;
                            if (task != null)
                            {
                                var validationResult = await task;

                                if (!validationResult.IsValid)
                                {
                                    context.Result = new BadRequestObjectResult(new
                                    {
                                        errors = validationResult.Errors.Select(e => new
                                        {
                                            property = e.PropertyName,
                                            error = e.ErrorMessage
                                        })
                                    });
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        await next();
    }
}

