using MediatR;
using Microsoft.Extensions.Logging;
using WareHouse.Application.DTOs;
using WareHouse.Domain.Entities;
using WareHouse.Domain.Enums;
using WareHouse.Domain.Exceptions;
using WareHouse.Domain.Interfaces;

namespace WareHouse.Application.Queries;

public record GetPickingTasksQuery(string Status, string Zone) : IRequest<List<PickingTaskDto>>;

public class GetPickingTasksQueryHandler : IRequestHandler<GetPickingTasksQuery, List<PickingTaskDto>>
{
    private readonly IPickingTaskRepository _pickingTaskRepository;
    private readonly ILogger<GetPickingTasksQueryHandler> _logger;

    public GetPickingTasksQueryHandler(
        IPickingTaskRepository pickingTaskRepository,
        ILogger<GetPickingTasksQueryHandler> logger)
    {
        _pickingTaskRepository = pickingTaskRepository;
        _logger = logger;
    }

    public async Task<List<PickingTaskDto>> Handle(GetPickingTasksQuery request, CancellationToken cancellationToken)
    {
        List<PickingTask> tasks;

        _logger.LogInformation("Getting picking tasks. Status: {Status}, Zone: {Zone}", request.Status, request.Zone);

        if (!string.IsNullOrEmpty(request.Status) && !string.IsNullOrEmpty(request.Zone))
        {
            if (!Enum.TryParse<PickingTaskStatus>(request.Status, true, out var status))
                throw new DomainException($"Invalid picking task status: {request.Status}");

            tasks = await _pickingTaskRepository.GetTasksByZoneAsync(request.Zone);
            tasks = tasks.Where(t => t.Status == status).ToList();
        }
        else if (!string.IsNullOrEmpty(request.Status))
        {
            if (!Enum.TryParse<PickingTaskStatus>(request.Status, true, out var status))
                throw new DomainException($"Invalid picking task status: {request.Status}");

            tasks = await _pickingTaskRepository.GetTasksByStatusAsync(status);
        }
        else if (!string.IsNullOrEmpty(request.Zone))
        {
            tasks = await _pickingTaskRepository.GetTasksByZoneAsync(request.Zone);
        }
        else
        {
            tasks = (List<PickingTask>)await _pickingTaskRepository.GetAllAsync();
        }

        _logger.LogInformation("Found {TaskCount} picking tasks", tasks.Count);

        // ✅ УБЕДИТЕСЬ, ЧТО ВСЕ ЗАДАЧИ ИМЕЮТ ЗАГРУЖЕННЫЕ ITEMS
        foreach (var task in tasks)
        {
            _logger.LogDebug("Task {TaskId} has {ItemCount} items", task.TaskId, task.Items?.Count ?? 0);
        }

        return tasks.Select(PickingTaskDto.FromEntity).ToList();
    }
}