namespace UserService.API.Controllers;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

[ApiController]
public class ErrorController : ControllerBase
{
    private readonly ILogger<ErrorController> _logger;

    public ErrorController(ILogger<ErrorController> logger)
    {
        _logger = logger;
    }

    [Route("/error")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult Error()
    {
        var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
        var exception = context?.Error;

        if (exception != null)
        {
            _logger.LogError(exception, "An unhandled exception occurred");

            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "An error occurred while processing your request",
                message = exception.Message
            });
        }

        return StatusCode(StatusCodes.Status500InternalServerError, new
        {
            error = "An error occurred while processing your request"
        });
    }

    [Route("/error/{statusCode}")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult Error(int statusCode)
    {
        var context = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();

        return statusCode switch
        {
            404 => NotFound(new { error = "Resource not found" }),
            400 => BadRequest(new { error = "Bad request" }),
            401 => Unauthorized(new { error = "Unauthorized" }),
            403 => StatusCode(StatusCodes.Status403Forbidden, new { error = "Forbidden" }),
            _ => StatusCode(statusCode, new { error = $"An error occurred with status code {statusCode}" })
        };
    }
}

