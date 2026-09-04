using TodoApp.Application.Dtos;
using TodoApp.Application.Repositories;
using TodoApp.Domain.Entities;
using TodoApp.Domain.Enums;
using TodoApp.Domain.ValueObjects;

namespace TodoApp.Application.Services;

public sealed class TodoService : ITodoService
{
    private readonly ITodoItemRepository _todoItemRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IActivityLogRepository _activityLogRepository;

    public TodoService(
        ITodoItemRepository todoItemRepository,
        IProjectRepository projectRepository,
        IActivityLogRepository activityLogRepository)
    {
        _todoItemRepository = todoItemRepository;
        _projectRepository = projectRepository;
        _activityLogRepository = activityLogRepository;
    }

    public async Task<IReadOnlyCollection<TodoItemDto>> ListAsync(TodoQueryDto query, CancellationToken cancellationToken = default)
    {
        var items = await _todoItemRepository.ListAsync(cancellationToken);
        var nowUtc = DateTimeOffset.UtcNow;
        var todayUtc = nowUtc.Date;
        var upcomingCutoff = todayUtc.AddDays(7);

        var filteredItems = items.Where(item => MatchesProject(item, query))
            .Where(item => MatchesStatus(item, query))
            .Where(item => MatchesPriority(item, query))
            .Where(item => MatchesTag(item, query))
            .Where(item => MatchesSmartFilter(item, query.SmartFilter, todayUtc, upcomingCutoff))
            .OrderBy(item => item.DueDateUtc ?? DateTimeOffset.MaxValue)
            .ThenByDescending(item => item.Priority)
            .Select(item => item.ToDto())
            .ToList();

        return filteredItems;
    }

    public async Task<TodoItemDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await _todoItemRepository.GetByIdAsync(id, cancellationToken);
        return item?.ToDto();
    }

    public async Task<TodoItemDto> CreateAsync(CreateTodoItemDto request, string actor, CancellationToken cancellationToken = default)
    {
        var nowUtc = DateTimeOffset.UtcNow;
        var projectId = await ResolveProjectIdAsync(request.ProjectId, cancellationToken);
        var recurrence = BuildRecurrence(request.Recurrence);

        var item = TodoItem.Create(
            Guid.NewGuid(),
            projectId,
            request.Title,
            request.Description,
            request.Status,
            request.Priority,
            request.Tags,
            request.DueDateUtc,
            request.RemindersUtc,
            recurrence,
            actor,
            nowUtc);

        await _todoItemRepository.AddAsync(item, cancellationToken);
        await _activityLogRepository.AddAsync(
            ActivityLogEntry.Create(
                "Task",
                item.Id,
                "Created",
                actor,
                nowUtc,
                $"Created task '{item.Title}' in project '{projectId}'."),
            cancellationToken);

        return item.ToDto();
    }

    public async Task<TodoItemDto?> UpdateAsync(Guid id, UpdateTodoItemDto request, string actor, CancellationToken cancellationToken = default)
    {
        var existingItem = await _todoItemRepository.GetByIdAsync(id, cancellationToken);
        if (existingItem is null)
        {
            return null;
        }

        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project is null)
        {
            throw new InvalidOperationException($"Project '{request.ProjectId}' was not found.");
        }

        var recurrence = BuildRecurrence(request.Recurrence);
        var nowUtc = DateTimeOffset.UtcNow;

        existingItem.Update(
            request.ProjectId,
            request.Title,
            request.Description,
            request.Status,
            request.Priority,
            request.Tags,
            request.DueDateUtc,
            request.RemindersUtc,
            recurrence,
            actor,
            nowUtc);

        await _todoItemRepository.UpdateAsync(existingItem, cancellationToken);
        await _activityLogRepository.AddAsync(
            ActivityLogEntry.Create(
                "Task",
                existingItem.Id,
                "Updated",
                actor,
                nowUtc,
                $"Updated task '{existingItem.Title}' (status: {existingItem.Status}, priority: {existingItem.Priority})."),
            cancellationToken);

        return existingItem.ToDto();
    }

    public async Task<bool> DeleteAsync(Guid id, string actor, CancellationToken cancellationToken = default)
    {
        var existingItem = await _todoItemRepository.GetByIdAsync(id, cancellationToken);
        if (existingItem is null)
        {
            return false;
        }

        var deleted = await _todoItemRepository.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return false;
        }

        await _activityLogRepository.AddAsync(
            ActivityLogEntry.Create(
                "Task",
                id,
                "Deleted",
                actor,
                DateTimeOffset.UtcNow,
                $"Deleted task '{existingItem.Title}'."),
            cancellationToken);

        return true;
    }

    private async Task<Guid> ResolveProjectIdAsync(Guid? requestedProjectId, CancellationToken cancellationToken)
    {
        if (requestedProjectId is { } projectId)
        {
            var project = await _projectRepository.GetByIdAsync(projectId, cancellationToken);
            if (project is null)
            {
                throw new InvalidOperationException($"Project '{projectId}' was not found.");
            }

            return projectId;
        }

        var existingProject = (await _projectRepository.ListAsync(cancellationToken)).FirstOrDefault();
        if (existingProject is null)
        {
            throw new InvalidOperationException("No project is available to assign this task.");
        }

        return existingProject.Id;
    }

    private static RecurrencePattern? BuildRecurrence(RecurrencePatternInputDto? recurrence)
    {
        if (recurrence is null || recurrence.Frequency == RecurrenceFrequency.None)
        {
            return null;
        }

        return new RecurrencePattern(recurrence.Frequency, recurrence.Interval);
    }

    private static bool MatchesProject(TodoItem item, TodoQueryDto query) =>
        !query.ProjectId.HasValue || item.ProjectId == query.ProjectId;

    private static bool MatchesStatus(TodoItem item, TodoQueryDto query) =>
        !query.Status.HasValue || item.Status == query.Status;

    private static bool MatchesPriority(TodoItem item, TodoQueryDto query) =>
        !query.Priority.HasValue || item.Priority == query.Priority;

    private static bool MatchesTag(TodoItem item, TodoQueryDto query)
    {
        if (string.IsNullOrWhiteSpace(query.Tag))
        {
            return true;
        }

        return item.Tags.Contains(query.Tag.Trim(), StringComparer.OrdinalIgnoreCase);
    }

    private static bool MatchesSmartFilter(
        TodoItem item,
        SmartFilterType smartFilter,
        DateTime todayUtc,
        DateTime upcomingCutoffUtc)
    {
        if (smartFilter == SmartFilterType.None)
        {
            return true;
        }

        var dueDate = item.DueDateUtc?.UtcDateTime.Date;
        if (!dueDate.HasValue)
        {
            return false;
        }

        return smartFilter switch
        {
            SmartFilterType.Today => dueDate.Value == todayUtc,
            SmartFilterType.Overdue => dueDate.Value < todayUtc && item.Status != TodoStatus.Done,
            SmartFilterType.Upcoming => dueDate.Value > todayUtc && dueDate.Value <= upcomingCutoffUtc,
            _ => true
        };
    }
}
