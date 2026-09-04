using TodoApp.Domain.Enums;

namespace TodoApp.Application.Dtos;

public sealed class TodoItemDto
{
    public Guid Id { get; init; }

    public Guid ProjectId { get; init; }

    public string Title { get; init; } = string.Empty;

    public string? Description { get; init; }

    public TodoStatus Status { get; init; }

    public TodoPriority Priority { get; init; }

    public IReadOnlyCollection<string> Tags { get; init; } = [];

    public DateTimeOffset? DueDateUtc { get; init; }

    public IReadOnlyCollection<DateTimeOffset> RemindersUtc { get; init; } = [];

    public RecurrencePatternDto? Recurrence { get; init; }

    public DateTimeOffset CreatedAtUtc { get; init; }

    public DateTimeOffset UpdatedAtUtc { get; init; }

    public string CreatedBy { get; init; } = string.Empty;

    public string UpdatedBy { get; init; } = string.Empty;
}
