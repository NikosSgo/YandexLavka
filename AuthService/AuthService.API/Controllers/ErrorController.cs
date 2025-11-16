using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers;

[ApiController]
public class ErrorController : ControllerBase
{
    [Route("/error")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult Error()
    {
        var context = HttpContext.Features.Get<IExceptionHandlerFeature>();
        var exception = context?.Error;

        return Problem(
            detail: exception?.Message,
            title: "An error occurred while processing your request",
            statusCode: StatusCodes.Status500InternalServerError
        );
    }

    [Route("/error/{statusCode}")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult Error(int statusCode)
    {
        return Problem(
            title: "An error occurred",
            statusCode: statusCode
        );
    }
}


