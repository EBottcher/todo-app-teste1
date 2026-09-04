namespace TodoApp.Application.Dtos;

public sealed class ActivityLogDto
{
    public Guid Id { get; init; }

    public string EntityType { get; init; } = string.Empty;

    public Guid EntityId { get; init; }

    public string Action { get; init; } = string.Empty;

    public string ChangedBy { get; init; } = string.Empty;

    public DateTimeOffset ChangedAtUtc { get; init; }

    public string Details { get; init; } = string.Empty;
}
