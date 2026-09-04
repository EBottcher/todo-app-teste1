using System.ComponentModel.DataAnnotations;
using TodoApp.Domain.Enums;

namespace TodoApp.Application.Dtos;

public sealed class CreateTodoItemDto
{
    public Guid? ProjectId { get; init; }

    [Required]
    [MaxLength(200)]
    public string Title { get; init; } = string.Empty;

    [MaxLength(2000)]
    public string? Description { get; init; }

    [Required]
    public TodoStatus Status { get; init; } = TodoStatus.Todo;

    [Required]
    public TodoPriority Priority { get; init; } = TodoPriority.Medium;

    public IReadOnlyCollection<string> Tags { get; init; } = [];

    public DateTimeOffset? DueDateUtc { get; init; }

    public IReadOnlyCollection<DateTimeOffset> RemindersUtc { get; init; } = [];

    public RecurrencePatternInputDto? Recurrence { get; init; }
}
