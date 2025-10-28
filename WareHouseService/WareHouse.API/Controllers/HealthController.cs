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
    public async Task<ActionResult> GetDetailed()
    {
        _logger.LogInformation("Detailed health check requested");

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
}