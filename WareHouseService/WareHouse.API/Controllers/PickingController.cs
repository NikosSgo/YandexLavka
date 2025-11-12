using MediatR;
using Microsoft.AspNetCore.Mvc;
using WareHouse.Application.DTOs;
using WareHouse.Application.Queries;

namespace WareHouse.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PickingController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PickingController> _logger;

    public PickingController(IMediator mediator, ILogger<PickingController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpGet("tasks")]
    [ProducesResponseType(typeof(List<PickingTaskDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PickingTaskDto>>> GetPickingTasks(
        [FromQuery] string status = null,
        [FromQuery] string zone = null)
    {
        _logger.LogInformation("Getting picking tasks. Status: {Status}, Zone: {Zone}", status, zone);

        var tasks = await _mediator.Send(new GetPickingTasksQuery(status, zone));
        return Ok(tasks);
    }

    [HttpGet("tasks/{taskId:guid}")]
    [ProducesResponseType(typeof(PickingTaskDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PickingTaskDto>> GetPickingTask(Guid taskId)
    {
        _logger.LogInformation("Getting picking task {TaskId}", taskId);

        // Для простоты используем существующий query
        var tasks = await _mediator.Send(new GetPickingTasksQuery(null, null));
        var task = tasks.FirstOrDefault(t => t.TaskId == taskId);

        if (task == null)
            return NotFound();

        return Ok(task);
    }

    [HttpGet("pickers/{pickerId}/tasks")]
    [ProducesResponseType(typeof(List<PickingTaskDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PickingTaskDto>>> GetPickerTasks(string pickerId)
    {
        _logger.LogInformation("Getting tasks for picker {PickerId}", pickerId);

        var tasks = await _mediator.Send(new GetPickingTasksQuery(null, null));
        var pickerTasks = tasks.Where(t => t.AssignedPicker == pickerId).ToList();

        return Ok(pickerTasks);
    }
}