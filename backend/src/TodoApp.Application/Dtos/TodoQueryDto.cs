using TodoApp.Domain.Enums;

namespace TodoApp.Application.Dtos;

public sealed class TodoQueryDto
{
    public Guid? ProjectId { get; init; }

    public TodoStatus? Status { get; init; }

    public TodoPriority? Priority { get; init; }

    public string? Tag { get; init; }

    public SmartFilterType SmartFilter { get; init; } = SmartFilterType.None;
}
