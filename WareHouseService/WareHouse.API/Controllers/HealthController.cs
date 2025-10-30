using Microsoft.AspNetCore.Mvc;

namespace WareHouse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult Get()
    {
        _logger.LogInformation("Health check requested");

        return Ok(new
        {
            status = "Healthy",
            timestamp = DateTime.UtcNow,
            service = "WareHouse.API",
            version = "1.0.0"
        });
    }

    [HttpGet("detailed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<ActionResult> GetDetailed()
    {
        _logger.LogInformation("Detailed health check requested");

        try
        {
            // Здесь можно добавить проверки различных зависимостей
            var checks = new
            {
                database = "Healthy",
                kafka = "Healthy",
                redis = "Healthy",
                external_services = "Healthy"
            };

            return Ok(new
            {
                status = "Healthy",
                timestamp = DateTime.UtcNow,
                checks
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Detailed health check failed");
            return StatusCode(503, new
            {
                status = "Unhealthy",
                timestamp = DateTime.UtcNow,
                error = ex.Message
            });
        }
    }
}